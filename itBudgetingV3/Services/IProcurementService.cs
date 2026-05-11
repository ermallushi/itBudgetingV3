using itBudgetingV3.Models;

namespace itBudgetingV3.Services;

public interface IProcurementService
{
    // PR
    Task<IEnumerable<PurchaseRequest>> GetPRsByBudgetLineAsync(int budgetLineId);
    Task<PurchaseRequest?> GetPRByIdAsync(int id);
    Task<PurchaseRequest> CreatePRAsync(PurchaseRequest pr, string userName);
    Task<PurchaseRequest> UpdatePRAsync(PurchaseRequest pr, string userName);
    Task<bool> ApprovePRAsync(int id, string userName);
    Task<bool> CancelPRAsync(int id, string userName);

    // PO
    Task<IEnumerable<PurchaseOrder>> GetPOsByPRAsync(int prId);
    Task<PurchaseOrder?> GetPOByIdAsync(int id);
    Task<PurchaseOrder> CreatePOAsync(PurchaseOrder po, string userName);
    Task<PurchaseOrder> UpdatePOAsync(PurchaseOrder po, string userName);
    Task<bool> CancelPOAsync(int id, string userName);

    // Budget consumption
    Task<decimal> GetTotalCommittedAsync(int budgetLineId);
    Task<decimal> GetTotalRemainingBudgetAsync(int budgetLineId);
    Task<bool> IsOverBudgetAsync(int budgetLineId);
}
