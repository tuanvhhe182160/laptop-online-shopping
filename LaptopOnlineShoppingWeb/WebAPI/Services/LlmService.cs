using System.Text;
using System.Text.Json;
using WebAPI.DTOs.AI;

namespace WebAPI.Services
{
    public class LlmService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public LlmService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<List<AiRecommendationItem>> GetRecommendationsAsync(
    string systemPrompt,
    string userPrompt)
        {
            var client = _httpClientFactory.CreateClient();

            string apiKey = _config["AiApi:ApiKey"]
                ?? throw new InvalidOperationException("API Key cho AI Service chưa được cấu hình.");;

            client.DefaultRequestHeaders.Add(
                "x-goog-api-key",
                apiKey
            );

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                new
                {
                    text = systemPrompt
                }
            }
                },

                contents = new[]
                {
            new
            {
                role = "user",
                parts = new[]
                {
                    new
                    {
                        text = userPrompt
                    }
                }
            }
        },

                generationConfig = new
                {
                    temperature = 0.2
                }
            };


            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );


            var response = await client.PostAsync(
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent",
                content
            );


            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }


            var responseData = await response.Content.ReadAsStringAsync();


            using var document = JsonDocument.Parse(responseData);


            var aiText = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";


            var cleanJson = aiText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            return JsonSerializer.Deserialize<List<AiRecommendationItem>>(
                cleanJson,
                options
            ) ?? new List<AiRecommendationItem>();
        }
    }
}
