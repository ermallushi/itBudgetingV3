using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace itBudgetingV3.Models;

public enum TransferStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class BudgetTransfer
{
    public int Id { get; set; }

    [Required]
    public int BudgetVersionId { get; set; }
    public BudgetVersion BudgetVersion { get; set; } = null!;

    // Source
    [Required]
    public int SourceBudgetLineId { get; set; }
    public BudgetLine SourceBudgetLine { get; set; } = null!;

    // Destination
    [Required]
    public int TargetBudgetLineId { get; set; }
    public BudgetLine TargetBudgetLine { get; set; } = null!;

    [Range(1, 12)]
    public int Month { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public TransferStatus Status { get; set; } = TransferStatus.Pending;

    /// <summary>True when CapEx ↔ OpEx transfer (requires approval)</summary>
    public bool IsCrossCategory { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    [MaxLength(256)]
    public string? ApprovedBy { get; set; }
}
