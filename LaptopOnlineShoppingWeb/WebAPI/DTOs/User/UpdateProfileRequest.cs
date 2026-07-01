namespace WebAPI.DTOs.User
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
