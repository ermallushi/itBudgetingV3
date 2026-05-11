using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace itBudgetingV3.Models;

public enum POStatus
{
    Open = 0,
    PartiallyUsed = 1,
    FullyUsed = 2,
    Cancelled = 3
}

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required]
    public int PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = null!;

    [Required, MaxLength(50)]
    public string PONumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ApprovedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UsedAmount { get; set; }

    [NotMapped]
    public decimal RemainingAmount => ApprovedAmount - UsedAmount;

    public POStatus Status { get; set; } = POStatus.Open;

    [MaxLength(500)]
    public string? SharePointUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? CreatedBy { get; set; }
}
