namespace WebAPI.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBranches { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
