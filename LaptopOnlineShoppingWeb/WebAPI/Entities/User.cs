using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordExpiry { get; set; }

    public bool? IsActive { get; set; }

    public int RoleId { get; set; }

    public int? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Role Role { get; set; } = null!;
}
