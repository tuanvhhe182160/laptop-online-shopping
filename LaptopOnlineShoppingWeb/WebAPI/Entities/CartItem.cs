using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class CartItem
{
    public int CartId { get; set; }

    public int VariantId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
