namespace WebAPI.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public bool IsGoogleAccount { get; set; }
    }
}
