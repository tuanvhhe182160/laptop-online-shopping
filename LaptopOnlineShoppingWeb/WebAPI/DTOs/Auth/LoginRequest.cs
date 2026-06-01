using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Username.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Password.")]
        public string Password { get; set; } = string.Empty;
    }
}
