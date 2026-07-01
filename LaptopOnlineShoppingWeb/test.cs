using System;
using System.Text.Json;

public class ErrorResponse { public string Message { get; set; } }

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>System.ArgumentOutOfRangeException</h1></body></html>";
        try
        {
            var errorObj = JsonSerializer.Deserialize<ErrorResponse>(html, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine("Success: " + errorObj?.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
        }
    }
}
