using System.ComponentModel.DataAnnotations;

namespace itBudgetingV3.Models;

public enum BudgetVersionStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Active = 3
}

public enum RevisionType
{
    Initial_0_12 = 0,  // 0+12
    Q1_3_9 = 1,        // 3+9
    MidYear_5_7 = 2,   // 5+7
    Final_9_3 = 3      // 9+3
}

public class BudgetVersion
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g. 2026_PLAN_0_12

    [Required]
    public int Year { get; set; }

    public BudgetVersionStatus Status { get; set; } = BudgetVersionStatus.Draft;

    public RevisionType RevisionType { get; set; } = RevisionType.Initial_0_12;

    public int ClosedMonths { get; set; } = 0;  // X in X+Y
    public int OpenMonths { get; set; } = 12;   // Y in X+Y

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    [MaxLength(256)]
    public string? ApprovedBy { get; set; }

    public int? ParentVersionId { get; set; }
    public BudgetVersion? ParentVersion { get; set; }

    // Navigation
    public ICollection<BudgetLine> BudgetLines { get; set; } = new List<BudgetLine>();
    public ICollection<BudgetPeriod> Periods { get; set; } = new List<BudgetPeriod>();
}
