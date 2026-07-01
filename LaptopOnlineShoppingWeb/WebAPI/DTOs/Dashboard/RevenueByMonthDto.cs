namespace WebAPI.DTOs.Dashboard
{
    public class RevenueByMonthDto
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
    }
}
