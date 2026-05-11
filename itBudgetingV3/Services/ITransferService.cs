using itBudgetingV3.Models;

namespace itBudgetingV3.Services;

public interface ITransferService
{
    Task<IEnumerable<BudgetTransfer>> GetByVersionAsync(int versionId);
    Task<BudgetTransfer?> GetByIdAsync(int id);
    Task<BudgetTransfer> CreateTransferAsync(BudgetTransfer transfer, string userName);
    Task<bool> ApproveTransferAsync(int id, string userName);
    Task<bool> RejectTransferAsync(int id, string userName);
}
