using itBudgetingV3.Data;
using itBudgetingV3.Models;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Services;

public class TransferService : ITransferService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public TransferService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IEnumerable<BudgetTransfer>> GetByVersionAsync(int versionId)
        => await _db.BudgetTransfers
            .Include(t => t.SourceBudgetLine)
            .Include(t => t.TargetBudgetLine)
            .Where(t => t.BudgetVersionId == versionId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<BudgetTransfer?> GetByIdAsync(int id)
        => await _db.BudgetTransfers
            .Include(t => t.SourceBudgetLine)
            .Include(t => t.TargetBudgetLine)
            .Include(t => t.BudgetVersion)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<BudgetTransfer> CreateTransferAsync(BudgetTransfer transfer, string userName)
    {
        var version = await _db.BudgetVersions.FindAsync(transfer.BudgetVersionId)
            ?? throw new InvalidOperationException("Version not found.");

        if (version.Status == BudgetVersionStatus.Approved || version.Status == BudgetVersionStatus.Active)
            throw new InvalidOperationException("Cannot transfer in an approved/active version.");

        // Validate period is open
        var period = await _db.BudgetPeriods.FirstOrDefaultAsync(p =>
            p.BudgetVersionId == transfer.BudgetVersionId && p.Month == transfer.Month);
        if (period == null || !period.IsOpen)
            throw new InvalidOperationException("Target period is closed.");

        var src = await _db.BudgetLines.FindAsync(transfer.SourceBudgetLineId)
            ?? throw new InvalidOperationException("Source budget line not found.");
        var tgt = await _db.BudgetLines.FindAsync(transfer.TargetBudgetLineId)
            ?? throw new InvalidOperationException("Target budget line not found.");

        // Cross-category check
        transfer.IsCrossCategory = src.Category != tgt.Category;

        // Source must have available budget for that month
        decimal available = src.GetMonth(transfer.Month);
        if (transfer.Amount > available)
            throw new InvalidOperationException(
                $"Insufficient budget in source line for month {transfer.Month}. Available: {available:N2}, Requested: {transfer.Amount:N2}");

        transfer.CreatedBy = userName;
        transfer.CreatedAt = DateTime.UtcNow;
        transfer.Status = transfer.IsCrossCategory ? TransferStatus.Pending : TransferStatus.Approved;

        // Apply immediately if same-category (cross-category needs manager approval)
        if (!transfer.IsCrossCategory)
        {
            src.SetMonth(transfer.Month, src.GetMonth(transfer.Month) - transfer.Amount);
            tgt.SetMonth(transfer.Month, tgt.GetMonth(transfer.Month) + transfer.Amount);
            src.UpdatedAt = DateTime.UtcNow;
            src.UpdatedBy = userName;
            tgt.UpdatedAt = DateTime.UtcNow;
            tgt.UpdatedBy = userName;
            transfer.ApprovedAt = DateTime.UtcNow;
            transfer.ApprovedBy = userName;
        }

        _db.BudgetTransfers.Add(transfer);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetTransfer", transfer.Id, "Create", changedBy: userName,
            notes: $"Transfer {transfer.Amount:N2} from line {src.Id} to {tgt.Id}, month={transfer.Month}");

        return transfer;
    }

    public async Task<bool> ApproveTransferAsync(int id, string userName)
    {
        var transfer = await _db.BudgetTransfers
            .Include(t => t.SourceBudgetLine)
            .Include(t => t.TargetBudgetLine)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null || transfer.Status != TransferStatus.Pending) return false;

        var src = transfer.SourceBudgetLine;
        var tgt = transfer.TargetBudgetLine;

        src.SetMonth(transfer.Month, src.GetMonth(transfer.Month) - transfer.Amount);
        tgt.SetMonth(transfer.Month, tgt.GetMonth(transfer.Month) + transfer.Amount);
        src.UpdatedAt = DateTime.UtcNow;
        src.UpdatedBy = userName;
        tgt.UpdatedAt = DateTime.UtcNow;
        tgt.UpdatedBy = userName;

        transfer.Status = TransferStatus.Approved;
        transfer.ApprovedAt = DateTime.UtcNow;
        transfer.ApprovedBy = userName;

        await _db.SaveChangesAsync();
        await _audit.LogAsync("BudgetTransfer", id, "Approve",
            oldValue: "Pending", newValue: "Approved", changedBy: userName);
        return true;
    }

    public async Task<bool> RejectTransferAsync(int id, string userName)
    {
        var transfer = await _db.BudgetTransfers.FindAsync(id);
        if (transfer == null || transfer.Status != TransferStatus.Pending) return false;

        transfer.Status = TransferStatus.Rejected;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("BudgetTransfer", id, "Reject",
            oldValue: "Pending", newValue: "Rejected", changedBy: userName);
        return true;
    }
}
