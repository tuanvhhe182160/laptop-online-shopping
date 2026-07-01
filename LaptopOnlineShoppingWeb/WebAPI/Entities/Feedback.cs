using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int CustomerId { get; set; }

    public int VariantId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsHidden { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
