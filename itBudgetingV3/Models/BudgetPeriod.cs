using System.ComponentModel.DataAnnotations;

namespace itBudgetingV3.Models;

public class BudgetPeriod
{
    public int Id { get; set; }

    [Required]
    public int BudgetVersionId { get; set; }
    public BudgetVersion BudgetVersion { get; set; } = null!;

    [Range(1, 12)]
    public int Month { get; set; }   // 1=Jan … 12=Dec

    public bool IsOpen { get; set; } = true;

    public DateTime? ClosedAt { get; set; }

    [MaxLength(256)]
    public string? ClosedBy { get; set; }
}
