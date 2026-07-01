using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("OrderDetails")]
[PrimaryKey(nameof(OrderId), nameof(VariantId))]
public partial class OrderDetail
{
    public int OrderId { get; set; }

    [ForeignKey("ProductVariant")]
    public int VariantId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    public virtual Order Order { get; set; } = null!;

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
