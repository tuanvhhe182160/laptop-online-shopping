namespace WebClient.ViewModels.User
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = "";

        public string Password { get; set; } = "";

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public int RoleId { get; set; }

        public int? BranchId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
