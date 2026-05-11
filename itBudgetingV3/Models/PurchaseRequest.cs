using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace itBudgetingV3.Models;

public enum PRStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Cancelled = 3
}

public class PurchaseRequest
{
    public int Id { get; set; }

    [Required]
    public int BudgetLineId { get; set; }
    public BudgetLine BudgetLine { get; set; } = null!;

    [Required, MaxLength(50)]
    public string PRNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public PRStatus Status { get; set; } = PRStatus.Draft;

    [MaxLength(500)]
    public string? SharePointUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    // Navigation
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
