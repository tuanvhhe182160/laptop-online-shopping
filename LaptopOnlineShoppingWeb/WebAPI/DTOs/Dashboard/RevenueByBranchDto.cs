namespace WebAPI.DTOs.Dashboard
{
    public class RevenueByBranchDto
    {
        public string BranchName { get; set; } = null!;
        public decimal Revenue { get; set; }
    }
}
