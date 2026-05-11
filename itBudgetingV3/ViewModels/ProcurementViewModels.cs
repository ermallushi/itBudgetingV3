using itBudgetingV3.Models;

namespace itBudgetingV3.ViewModels;

public class ProcurementDashboardViewModel
{
    public IEnumerable<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
    public int? FilterBudgetLineId { get; set; }
    public int? FilterVersionId { get; set; }
}

public class PRCreateViewModel
{
    public int BudgetLineId { get; set; }
    public string? BudgetLineInfo { get; set; }
    public decimal AvailableBudget { get; set; }
    public PurchaseRequest PR { get; set; } = new();
}

public class POCreateViewModel
{
    public int PurchaseRequestId { get; set; }
    public string? PRInfo { get; set; }
    public PurchaseOrder PO { get; set; } = new();
}

public class ReportViewModel
{
    public IEnumerable<BudgetVersion> Versions { get; set; } = new List<BudgetVersion>();
    public int? SelectedVersionId { get; set; }
    public IEnumerable<ReportRow> Rows { get; set; } = new List<ReportRow>();
    public decimal TotalBudget { get; set; }
    public decimal TotalCommitted { get; set; }
    public decimal TotalRemaining { get; set; }
    public decimal TotalCapEx { get; set; }
    public decimal TotalOpEx { get; set; }
}

public class ReportRow
{
    public string? Project { get; set; }
    public string? Vendor { get; set; }
    public string? CostCenter { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public decimal TotalCommitted { get; set; }
    public decimal TotalRemaining { get; set; }
    public bool IsOverBudget => TotalRemaining < 0;
}

public class TransferCreateViewModel
{
    public int BudgetVersionId { get; set; }
    public IEnumerable<BudgetLine> BudgetLines { get; set; } = new List<BudgetLine>();
    public IEnumerable<BudgetPeriod> Periods { get; set; } = new List<BudgetPeriod>();
    public BudgetTransfer Transfer { get; set; } = new();
}

public class TransferListViewModel
{
    public IEnumerable<BudgetTransfer> Transfers { get; set; } = new List<BudgetTransfer>();
    public BudgetVersion Version { get; set; } = null!;
}
