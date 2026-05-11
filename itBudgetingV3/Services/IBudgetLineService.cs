using itBudgetingV3.Models;

namespace itBudgetingV3.Services;

public interface IBudgetLineService
{
    Task<IEnumerable<BudgetLine>> GetByVersionAsync(int versionId, string? costCenter = null, BudgetCategory? category = null);
    Task<BudgetLine?> GetByIdAsync(int id);
    Task<BudgetLine> CreateAsync(BudgetLine line, string userName);
    Task<BudgetLine> UpdateAsync(BudgetLine line, string userName);
    Task<bool> DeleteAsync(int id, string userName);
    Task<bool> IsMonthEditableAsync(int budgetLineId, int month);
    Task<IEnumerable<BudgetLine>> ImportFromExcelAsync(int versionId, Stream fileStream, string userName);
    Task<(decimal TotalBudget, decimal TotalCommitted, decimal TotalRemaining)> GetBudgetSummaryAsync(int versionId);
}
