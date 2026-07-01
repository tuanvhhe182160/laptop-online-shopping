using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("CartItems")]
[PrimaryKey(nameof(CartId), nameof(VariantId))]
public partial class CartItem
{
    public int CartId { get; set; }

    [ForeignKey("ProductVariant")]
    public int VariantId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
