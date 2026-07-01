using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://localhost:7136/");
        // Ignore SSL errors
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

        // 1. Register
        var registerData = new { Username = "test1", Password = "password123", FullName = "Test 1", Email = "test1@gmail.com" };
        var registerContent = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");
        var regResponse = await client.PostAsync("api/auth/customer-register", registerContent);
        Console.WriteLine($"Register: {regResponse.StatusCode}");
        Console.WriteLine(await regResponse.Content.ReadAsStringAsync());

        // 2. Login
        var loginData = new { Username = "test1", Password = "password123" };
        var loginContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("api/auth/customer-login", loginContent);
        Console.WriteLine($"Login: {loginResponse.StatusCode}");
        Console.WriteLine(await loginResponse.Content.ReadAsStringAsync());
    }
}
