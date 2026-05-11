using itBudgetingV3.Models;

namespace itBudgetingV3.Services;

public interface IBudgetVersionService
{
    Task<IEnumerable<BudgetVersion>> GetAllAsync();
    Task<BudgetVersion?> GetByIdAsync(int id);
    Task<BudgetVersion> CreateAsync(BudgetVersion version, string userName);
    Task<BudgetVersion> CloneForRevisionAsync(int sourceVersionId, RevisionType revisionType, string userName);
    Task<bool> SubmitAsync(int id, string userName);
    Task<bool> ApproveAsync(int id, string userName);
    Task<bool> ActivateAsync(int id, string userName);
    Task<IEnumerable<BudgetPeriod>> GetPeriodsAsync(int versionId);
    Task<bool> TogglePeriodAsync(int versionId, int month, bool isOpen, string userName);
}
