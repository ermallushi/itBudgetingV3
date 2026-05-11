using System.ComponentModel.DataAnnotations;

namespace itBudgetingV3.Models;

public class CostCenter
{
    [Key, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<BudgetLine> BudgetLines { get; set; } = new List<BudgetLine>();
}
