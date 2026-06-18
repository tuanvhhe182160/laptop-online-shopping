using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class PhysicalProduct
{
    public int PhysicalId { get; set; }

    public int VariantId { get; set; }

    public int BranchId { get; set; }

    public int? OrderId { get; set; }

    public string SerialNumber { get; set; } = null!;

    public string? Status { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual ProductVariant Variant { get; set; } = null!;
}
