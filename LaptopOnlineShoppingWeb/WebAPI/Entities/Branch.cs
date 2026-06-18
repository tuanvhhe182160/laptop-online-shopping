using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class Branch
{
    public int BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public string? Address { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PhysicalProduct> PhysicalProducts { get; set; } = new List<PhysicalProduct>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
