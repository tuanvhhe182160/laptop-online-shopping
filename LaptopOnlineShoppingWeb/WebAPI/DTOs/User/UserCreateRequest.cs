using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.User
{
    public class UserCreateRequest
    {
        [Required] 
        public string Username { get; set; } = string.Empty;
        [Required] 
        public string Password { get; set; } = string.Empty;
        [Required] 
        public string FullName { get; set; } = string.Empty;
        [Required][EmailAddress] 
        public string Email { get; set; } = string.Empty;
        [Required] 
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
