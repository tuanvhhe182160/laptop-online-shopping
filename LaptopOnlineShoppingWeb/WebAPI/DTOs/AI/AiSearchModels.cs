using System.Text.Json.Serialization;

namespace WebAPI.DTOs.AI 
{
    public class AiSearchRequest
    {
        public string Needs { get; set; } = string.Empty;
        public decimal MaxBudget { get; set; }
        public string AdditionalNotes { get; set; } = string.Empty;
    }

    public class AiRecommendationItem
    {
        [JsonPropertyName("variantId")]
        public int VariantId { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }

    public class AiSearchResponse
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Cpu { get; set; } = string.Empty;
        public string Ram { get; set; } = string.Empty;
        public string Ssd { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string AiReason { get; set; } = string.Empty;
    }
}
