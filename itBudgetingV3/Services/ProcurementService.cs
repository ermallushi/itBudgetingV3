using itBudgetingV3.Data;
using itBudgetingV3.Models;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Services;

public class ProcurementService : IProcurementService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public ProcurementService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // ── PR ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PurchaseRequest>> GetPRsByBudgetLineAsync(int budgetLineId)
        => await _db.PurchaseRequests
            .Include(pr => pr.PurchaseOrders)
            .Where(pr => pr.BudgetLineId == budgetLineId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();

    public async Task<PurchaseRequest?> GetPRByIdAsync(int id)
        => await _db.PurchaseRequests
            .Include(pr => pr.PurchaseOrders)
            .Include(pr => pr.BudgetLine)
            .FirstOrDefaultAsync(pr => pr.Id == id);

    public async Task<PurchaseRequest> CreatePRAsync(PurchaseRequest pr, string userName)
    {
        pr.CreatedBy = userName;
        pr.CreatedAt = DateTime.UtcNow;
        pr.Status = PRStatus.Draft;
        _db.PurchaseRequests.Add(pr);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseRequest", pr.Id, "Create", changedBy: userName,
            notes: $"PR={pr.PRNumber}, Amount={pr.Amount:N2}");
        return pr;
    }

    public async Task<PurchaseRequest> UpdatePRAsync(PurchaseRequest pr, string userName)
    {
        var existing = await _db.PurchaseRequests.FindAsync(pr.Id)
            ?? throw new InvalidOperationException("PR not found.");
        if (existing.Status != PRStatus.Draft)
            throw new InvalidOperationException("Only draft PRs can be edited.");

        existing.PRNumber = pr.PRNumber;
        existing.Description = pr.Description;
        existing.Amount = pr.Amount;
        existing.SharePointUrl = pr.SharePointUrl;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseRequest", pr.Id, "Update", changedBy: userName);
        return existing;
    }

    public async Task<bool> ApprovePRAsync(int id, string userName)
    {
        var pr = await _db.PurchaseRequests.FindAsync(id);
        if (pr == null || pr.Status != PRStatus.Submitted) return false;
        pr.Status = PRStatus.Approved;
        pr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseRequest", id, "Approve",
            oldValue: "Submitted", newValue: "Approved", changedBy: userName);
        return true;
    }

    public async Task<bool> CancelPRAsync(int id, string userName)
    {
        var pr = await _db.PurchaseRequests.FindAsync(id);
        if (pr == null) return false;
        pr.Status = PRStatus.Cancelled;
        pr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseRequest", id, "Cancel", changedBy: userName);
        return true;
    }

    // ── PO ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PurchaseOrder>> GetPOsByPRAsync(int prId)
        => await _db.PurchaseOrders
            .Where(po => po.PurchaseRequestId == prId)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync();

    public async Task<PurchaseOrder?> GetPOByIdAsync(int id)
        => await _db.PurchaseOrders
            .Include(po => po.PurchaseRequest).ThenInclude(pr => pr.BudgetLine)
            .FirstOrDefaultAsync(po => po.Id == id);

    public async Task<PurchaseOrder> CreatePOAsync(PurchaseOrder po, string userName)
    {
        po.CreatedBy = userName;
        po.CreatedAt = DateTime.UtcNow;
        po.Status = POStatus.Open;
        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseOrder", po.Id, "Create", changedBy: userName,
            notes: $"PO={po.PONumber}, Approved={po.ApprovedAmount:N2}");
        return po;
    }

    public async Task<PurchaseOrder> UpdatePOAsync(PurchaseOrder po, string userName)
    {
        var existing = await _db.PurchaseOrders.FindAsync(po.Id)
            ?? throw new InvalidOperationException("PO not found.");
        existing.PONumber = po.PONumber;
        existing.Description = po.Description;
        existing.ApprovedAmount = po.ApprovedAmount;
        existing.UsedAmount = po.UsedAmount;
        existing.SharePointUrl = po.SharePointUrl;
        existing.Status = existing.UsedAmount >= existing.ApprovedAmount
            ? POStatus.FullyUsed
            : existing.UsedAmount > 0 ? POStatus.PartiallyUsed : POStatus.Open;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseOrder", po.Id, "Update", changedBy: userName);
        return existing;
    }

    public async Task<bool> CancelPOAsync(int id, string userName)
    {
        var po = await _db.PurchaseOrders.FindAsync(id);
        if (po == null) return false;
        po.Status = POStatus.Cancelled;
        po.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("PurchaseOrder", id, "Cancel", changedBy: userName);
        return true;
    }

    // ── Budget consumption ──────────────────────────────────────────────

    public async Task<decimal> GetTotalCommittedAsync(int budgetLineId)
    {
        // Get approved PR IDs for this budget line
        var approvedPRIds = await _db.PurchaseRequests
            .Where(pr => pr.BudgetLineId == budgetLineId && pr.Status == PRStatus.Approved)
            .Select(pr => pr.Id)
            .ToListAsync();

        if (!approvedPRIds.Any()) return 0;

        return await _db.PurchaseOrders
            .Where(po => approvedPRIds.Contains(po.PurchaseRequestId) && po.Status != POStatus.Cancelled)
            .SumAsync(po => po.ApprovedAmount);
    }

    public async Task<decimal> GetTotalRemainingBudgetAsync(int budgetLineId)
    {
        var line = await _db.BudgetLines.FindAsync(budgetLineId);
        if (line == null) return 0;
        var committed = await GetTotalCommittedAsync(budgetLineId);
        return (line.Jan + line.Feb + line.Mar + line.Apr + line.May + line.Jun +
                line.Jul + line.Aug + line.Sep + line.Oct + line.Nov + line.Dec) - committed;
    }

    public async Task<bool> IsOverBudgetAsync(int budgetLineId)
        => await GetTotalRemainingBudgetAsync(budgetLineId) < 0;
}
