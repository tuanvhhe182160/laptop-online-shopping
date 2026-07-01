namespace WebAPI.DTOs.User
{
    public class UserUpdateRequest
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int RoleId { get; set; }
        public int? BranchId { get; set; }
        public bool IsActive { get; set; }
    }
}
