using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordExpiry { get; set; }

    public bool? IsActive { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
