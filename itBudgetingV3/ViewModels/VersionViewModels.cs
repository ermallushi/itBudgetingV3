using itBudgetingV3.Models;

namespace itBudgetingV3.ViewModels;

public class VersionListViewModel
{
    public IEnumerable<BudgetVersion> Versions { get; set; } = new List<BudgetVersion>();
}

public class VersionCreateViewModel
{
    public int Year { get; set; } = DateTime.Now.Year;
    public RevisionType RevisionType { get; set; } = RevisionType.Initial_0_12;
    public string? Description { get; set; }
}

public class VersionCloneViewModel
{
    public int SourceVersionId { get; set; }
    public RevisionType RevisionType { get; set; }
    public IEnumerable<BudgetVersion> AvailableVersions { get; set; } = new List<BudgetVersion>();
}

public class PeriodManagementViewModel
{
    public BudgetVersion Version { get; set; } = null!;
    public IEnumerable<BudgetPeriod> Periods { get; set; } = new List<BudgetPeriod>();
}
