using itBudgetingV3.Data;
using itBudgetingV3.Models;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Services;

public class BudgetVersionService : IBudgetVersionService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public BudgetVersionService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IEnumerable<BudgetVersion>> GetAllAsync()
        => await _db.BudgetVersions
            .Include(v => v.Periods)
            .OrderByDescending(v => v.Year)
            .ThenBy(v => v.RevisionType)
            .ToListAsync();

    public async Task<BudgetVersion?> GetByIdAsync(int id)
        => await _db.BudgetVersions
            .Include(v => v.Periods)
            .Include(v => v.BudgetLines)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<BudgetVersion> CreateAsync(BudgetVersion version, string userName)
    {
        version.CreatedBy = userName;
        version.CreatedAt = DateTime.UtcNow;
        version.Status = BudgetVersionStatus.Draft;

        _db.BudgetVersions.Add(version);
        await _db.SaveChangesAsync();

        // Create 12 periods (all open)
        for (int m = 1; m <= 12; m++)
        {
            _db.BudgetPeriods.Add(new BudgetPeriod
            {
                BudgetVersionId = version.Id,
                Month = m,
                IsOpen = m > version.ClosedMonths
            });
        }
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetVersion", version.Id, "Create",
            changedBy: userName, notes: $"Created version {version.Name}");

        return version;
    }

    public async Task<BudgetVersion> CloneForRevisionAsync(int sourceVersionId, RevisionType revisionType, string userName)
    {
        var source = await GetByIdAsync(sourceVersionId)
            ?? throw new InvalidOperationException("Source version not found.");

        // Determine X+Y
        var (closed, open) = revisionType switch
        {
            RevisionType.Initial_0_12 => (0, 12),
            RevisionType.Q1_3_9 => (3, 9),
            RevisionType.MidYear_5_7 => (5, 7),
            RevisionType.Final_9_3 => (9, 3),
            _ => (0, 12)
        };

        var revisionLabel = revisionType switch
        {
            RevisionType.Initial_0_12 => "0_12",
            RevisionType.Q1_3_9 => "3_9",
            RevisionType.MidYear_5_7 => "5_7",
            RevisionType.Final_9_3 => "9_3",
            _ => "0_12"
        };

        var newVersion = new BudgetVersion
        {
            Name = $"{source.Year}_FORECAST_{revisionLabel}",
            Year = source.Year,
            RevisionType = revisionType,
            ClosedMonths = closed,
            OpenMonths = open,
            ParentVersionId = sourceVersionId,
            Description = $"Cloned from {source.Name}",
            Status = BudgetVersionStatus.Draft,
            CreatedBy = userName,
            CreatedAt = DateTime.UtcNow
        };

        _db.BudgetVersions.Add(newVersion);
        await _db.SaveChangesAsync();

        // Clone periods
        for (int m = 1; m <= 12; m++)
        {
            _db.BudgetPeriods.Add(new BudgetPeriod
            {
                BudgetVersionId = newVersion.Id,
                Month = m,
                IsOpen = m > closed
            });
        }

        // Clone budget lines
        foreach (var line in source.BudgetLines)
        {
            _db.BudgetLines.Add(new BudgetLine
            {
                BudgetVersionId = newVersion.Id,
                WBS = line.WBS,
                Project = line.Project,
                Vendor = line.Vendor,
                Category = line.Category,
                CostCenterCode = line.CostCenterCode,
                SAPCode = line.SAPCode,
                SAPDescription = line.SAPDescription,
                ITDomain = line.ITDomain,
                ITSubdomain = line.ITSubdomain,
                CommitmentCode = line.CommitmentCode,
                HyperionCategory = line.HyperionCategory,
                Requestor = line.Requestor,
                EPMOId = line.EPMOId,
                ContractDuration = line.ContractDuration,
                ITComments = line.ITComments,
                Jan = m(1) ? line.Jan : 0,
                Feb = m(2) ? line.Feb : 0,
                Mar = m(3) ? line.Mar : 0,
                Apr = m(4) ? line.Apr : 0,
                May = m(5) ? line.May : 0,
                Jun = m(6) ? line.Jun : 0,
                Jul = m(7) ? line.Jul : 0,
                Aug = m(8) ? line.Aug : 0,
                Sep = m(9) ? line.Sep : 0,
                Oct = m(10) ? line.Oct : 0,
                Nov = m(11) ? line.Nov : 0,
                Dec = m(12) ? line.Dec : 0,
                CreatedBy = userName,
                CreatedAt = DateTime.UtcNow
            });
            // local helper: keep value if month is closed (historical), zero if open
            bool m(int month) => month <= closed;
        }

        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetVersion", newVersion.Id, "Clone",
            changedBy: userName, notes: $"Cloned from version {source.Name} as {newVersion.Name}");

        return newVersion;
    }

    public async Task<bool> SubmitAsync(int id, string userName)
    {
        var v = await _db.BudgetVersions.FindAsync(id);
        if (v == null || v.Status != BudgetVersionStatus.Draft) return false;

        v.Status = BudgetVersionStatus.Submitted;
        v.SubmittedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetVersion", id, "Submit",
            oldValue: "Draft", newValue: "Submitted", changedBy: userName);
        return true;
    }

    public async Task<bool> ApproveAsync(int id, string userName)
    {
        var v = await _db.BudgetVersions.FindAsync(id);
        if (v == null || v.Status != BudgetVersionStatus.Submitted) return false;

        v.Status = BudgetVersionStatus.Approved;
        v.ApprovedAt = DateTime.UtcNow;
        v.ApprovedBy = userName;
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetVersion", id, "Approve",
            oldValue: "Submitted", newValue: "Approved", changedBy: userName);
        return true;
    }

    public async Task<bool> ActivateAsync(int id, string userName)
    {
        var v = await _db.BudgetVersions.FindAsync(id);
        if (v == null || v.Status != BudgetVersionStatus.Approved) return false;

        v.Status = BudgetVersionStatus.Active;
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetVersion", id, "Activate",
            oldValue: "Approved", newValue: "Active", changedBy: userName);
        return true;
    }

    public async Task<IEnumerable<BudgetPeriod>> GetPeriodsAsync(int versionId)
        => await _db.BudgetPeriods.Where(p => p.BudgetVersionId == versionId)
            .OrderBy(p => p.Month).ToListAsync();

    public async Task<bool> TogglePeriodAsync(int versionId, int month, bool isOpen, string userName)
    {
        var period = await _db.BudgetPeriods
            .FirstOrDefaultAsync(p => p.BudgetVersionId == versionId && p.Month == month);
        if (period == null) return false;

        var old = period.IsOpen;
        period.IsOpen = isOpen;
        if (!isOpen)
        {
            period.ClosedAt = DateTime.UtcNow;
            period.ClosedBy = userName;
        }
        else
        {
            period.ClosedAt = null;
            period.ClosedBy = null;
        }
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetPeriod", period.Id, "Toggle",
            fieldName: "IsOpen", oldValue: old.ToString(), newValue: isOpen.ToString(), changedBy: userName);
        return true;
    }
}
