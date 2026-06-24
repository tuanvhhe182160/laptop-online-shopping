using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace WebClient.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ResetPasswordModel(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Xác nhận mật khẩu không khớp.";
            return Page();
        }

        var client = _httpClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"{_configuration["ApiSettings:BaseUrl"]}api/auth/reset-password",
            Input);

        if (response.IsSuccessStatusCode)
        {
            Message = "Đặt lại mật khẩu thành công.";
            return Page();
        }

        ErrorMessage = await response.Content.ReadAsStringAsync();
        return Page();
    }

    public class ResetPasswordInput
    {
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}