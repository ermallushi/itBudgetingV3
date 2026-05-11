using itBudgetingV3.Models;

namespace itBudgetingV3.ViewModels;

public class BudgetGridViewModel
{
    public BudgetVersion Version { get; set; } = null!;
    public IEnumerable<BudgetLine> Lines { get; set; } = new List<BudgetLine>();
    public IEnumerable<BudgetPeriod> Periods { get; set; } = new List<BudgetPeriod>();
    public IEnumerable<CostCenter> CostCenters { get; set; } = new List<CostCenter>();
    public decimal TotalBudget { get; set; }
    public decimal TotalCommitted { get; set; }
    public decimal TotalRemaining { get; set; }

    // Filters
    public string? FilterCostCenter { get; set; }
    public BudgetCategory? FilterCategory { get; set; }
}

public class BudgetLineEditViewModel
{
    public BudgetLine Line { get; set; } = null!;
    public IEnumerable<CostCenter> CostCenters { get; set; } = new List<CostCenter>();
    public IEnumerable<BudgetPeriod> Periods { get; set; } = new List<BudgetPeriod>();
    public bool IsReadOnly { get; set; }
}

public class BudgetLineSaveDto
{
    public int Id { get; set; }
    public int BudgetVersionId { get; set; }
    public string? WBS { get; set; }
    public string? Project { get; set; }
    public string? Vendor { get; set; }
    public int Category { get; set; }
    public string CostCenterCode { get; set; } = string.Empty;
    public string? SAPCode { get; set; }
    public string? SAPDescription { get; set; }
    public string? ITDomain { get; set; }
    public string? ITSubdomain { get; set; }
    public string? CommitmentCode { get; set; }
    public string? HyperionCategory { get; set; }
    public string? Requestor { get; set; }
    public string? EPMOId { get; set; }
    public string? ContractDuration { get; set; }
    public string? ITComments { get; set; }
    public decimal Jan { get; set; }
    public decimal Feb { get; set; }
    public decimal Mar { get; set; }
    public decimal Apr { get; set; }
    public decimal May { get; set; }
    public decimal Jun { get; set; }
    public decimal Jul { get; set; }
    public decimal Aug { get; set; }
    public decimal Sep { get; set; }
    public decimal Oct { get; set; }
    public decimal Nov { get; set; }
    public decimal Dec { get; set; }
}
