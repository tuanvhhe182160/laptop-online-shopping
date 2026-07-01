namespace WebClient.ViewModels.User
{
    public class UserFormViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = "";

        // Chỉ dùng khi Create
        public string? Password { get; set; }

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public int RoleId { get; set; }

        public int? BranchId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
