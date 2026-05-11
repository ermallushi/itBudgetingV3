using ClosedXML.Excel;
using itBudgetingV3.Data;
using itBudgetingV3.Models;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Services;

public class BudgetLineService : IBudgetLineService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public BudgetLineService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IEnumerable<BudgetLine>> GetByVersionAsync(int versionId,
        string? costCenter = null, BudgetCategory? category = null)
    {
        var q = _db.BudgetLines
            .Include(l => l.CostCenter)
            .Include(l => l.PurchaseRequests).ThenInclude(pr => pr.PurchaseOrders)
            .Where(l => l.BudgetVersionId == versionId);

        if (costCenter != null)
            q = q.Where(l => l.CostCenterCode == costCenter);
        if (category.HasValue)
            q = q.Where(l => l.Category == category.Value);

        return await q.OrderBy(l => l.CostCenterCode).ThenBy(l => l.Project).ToListAsync();
    }

    public async Task<BudgetLine?> GetByIdAsync(int id)
        => await _db.BudgetLines
            .Include(l => l.CostCenter)
            .Include(l => l.PurchaseRequests).ThenInclude(pr => pr.PurchaseOrders)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<bool> IsMonthEditableAsync(int budgetLineId, int month)
    {
        var line = await _db.BudgetLines
            .Include(l => l.BudgetVersion).ThenInclude(v => v.Periods)
            .FirstOrDefaultAsync(l => l.Id == budgetLineId);

        if (line == null) return false;
        if (line.BudgetVersion.Status == BudgetVersionStatus.Approved ||
            line.BudgetVersion.Status == BudgetVersionStatus.Active) return false;

        var period = line.BudgetVersion.Periods.FirstOrDefault(p => p.Month == month);
        return period?.IsOpen ?? false;
    }

    public async Task<BudgetLine> CreateAsync(BudgetLine line, string userName)
    {
        var version = await _db.BudgetVersions.FindAsync(line.BudgetVersionId)
            ?? throw new InvalidOperationException("Version not found.");

        if (version.Status == BudgetVersionStatus.Approved || version.Status == BudgetVersionStatus.Active)
            throw new InvalidOperationException("Cannot add lines to an approved/active version.");

        line.CreatedBy = userName;
        line.CreatedAt = DateTime.UtcNow;
        _db.BudgetLines.Add(line);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BudgetLine", line.Id, "Create", changedBy: userName,
            notes: $"Project={line.Project}, Category={line.Category}");
        return line;
    }

    public async Task<BudgetLine> UpdateAsync(BudgetLine line, string userName)
    {
        var existing = await _db.BudgetLines
            .Include(l => l.BudgetVersion).ThenInclude(v => v.Periods)
            .FirstOrDefaultAsync(l => l.Id == line.Id)
            ?? throw new InvalidOperationException("Line not found.");

        if (existing.BudgetVersion.Status == BudgetVersionStatus.Approved ||
            existing.BudgetVersion.Status == BudgetVersionStatus.Active)
            throw new InvalidOperationException("Cannot edit an approved/active version.");

        // Enforce period locking per month
        var months = new (int num, decimal oldVal, decimal newVal)[]
        {
            (1,  existing.Jan, line.Jan),  (2,  existing.Feb, line.Feb),
            (3,  existing.Mar, line.Mar),  (4,  existing.Apr, line.Apr),
            (5,  existing.May, line.May),  (6,  existing.Jun, line.Jun),
            (7,  existing.Jul, line.Jul),  (8,  existing.Aug, line.Aug),
            (9,  existing.Sep, line.Sep),  (10, existing.Oct, line.Oct),
            (11, existing.Nov, line.Nov),  (12, existing.Dec, line.Dec),
        };

        foreach (var (num, _, newVal) in months)
        {
            var period = existing.BudgetVersion.Periods.FirstOrDefault(p => p.Month == num);
            if (period != null && !period.IsOpen)
                line.SetMonth(num, existing.GetMonth(num)); // preserve locked value
        }

        // Copy non-monthly fields
        existing.WBS = line.WBS;
        existing.Project = line.Project;
        existing.Vendor = line.Vendor;
        existing.Category = line.Category;
        existing.CostCenterCode = line.CostCenterCode;
        existing.SAPCode = line.SAPCode;
        existing.SAPDescription = line.SAPDescription;
        existing.ITDomain = line.ITDomain;
        existing.ITSubdomain = line.ITSubdomain;
        existing.CommitmentCode = line.CommitmentCode;
        existing.HyperionCategory = line.HyperionCategory;
        existing.Requestor = line.Requestor;
        existing.EPMOId = line.EPMOId;
        existing.ContractDuration = line.ContractDuration;
        existing.ITComments = line.ITComments;

        // Copy monthly values
        existing.Jan = line.Jan; existing.Feb = line.Feb; existing.Mar = line.Mar;
        existing.Apr = line.Apr; existing.May = line.May; existing.Jun = line.Jun;
        existing.Jul = line.Jul; existing.Aug = line.Aug; existing.Sep = line.Sep;
        existing.Oct = line.Oct; existing.Nov = line.Nov; existing.Dec = line.Dec;

        existing.UpdatedBy = userName;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _audit.LogAsync("BudgetLine", existing.Id, "Update", changedBy: userName);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, string userName)
    {
        var line = await _db.BudgetLines.Include(l => l.BudgetVersion)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (line == null) return false;

        if (line.BudgetVersion.Status == BudgetVersionStatus.Approved ||
            line.BudgetVersion.Status == BudgetVersionStatus.Active)
            return false;

        _db.BudgetLines.Remove(line);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("BudgetLine", id, "Delete", changedBy: userName);
        return true;
    }

    public async Task<(decimal TotalBudget, decimal TotalCommitted, decimal TotalRemaining)> GetBudgetSummaryAsync(int versionId)
    {
        var lines = await _db.BudgetLines
            .Include(l => l.PurchaseRequests).ThenInclude(pr => pr.PurchaseOrders)
            .Where(l => l.BudgetVersionId == versionId)
            .ToListAsync();

        decimal totalBudget = lines.Sum(l => l.Jan + l.Feb + l.Mar + l.Apr + l.May + l.Jun +
                                             l.Jul + l.Aug + l.Sep + l.Oct + l.Nov + l.Dec);

        decimal totalCommitted = lines.Sum(l =>
            l.PurchaseRequests
                .Where(pr => pr.Status == PRStatus.Approved)
                .Sum(pr => pr.PurchaseOrders
                    .Where(po => po.Status != POStatus.Cancelled)
                    .Sum(po => po.ApprovedAmount)));

        return (totalBudget, totalCommitted, totalBudget - totalCommitted);
    }

    public async Task<IEnumerable<BudgetLine>> ImportFromExcelAsync(int versionId, Stream fileStream, string userName)
    {
        var version = await _db.BudgetVersions.FindAsync(versionId)
            ?? throw new InvalidOperationException("Version not found.");

        if (version.Status == BudgetVersionStatus.Approved || version.Status == BudgetVersionStatus.Active)
            throw new InvalidOperationException("Cannot import into an approved/active version.");

        var imported = new List<BudgetLine>();

        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheets.First();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        // Detect CapEx vs OpEx by sheet name
        var sheetName = ws.Name.ToLower();
        var category = sheetName.Contains("opex") ? BudgetCategory.OpEx : BudgetCategory.CapEx;

        // Header row = 1
        for (int row = 2; row <= lastRow; row++)
        {
            var wbs = ws.Cell(row, 1).GetString().Trim();
            if (string.IsNullOrEmpty(wbs)) continue;

            var line = new BudgetLine
            {
                BudgetVersionId = versionId,
                Category = category,
                WBS = wbs,
                Project = ws.Cell(row, 2).GetString().Trim(),
                Vendor = ws.Cell(row, 3).GetString().Trim(),
                CostCenterCode = ws.Cell(row, 4).GetString().Trim(),
                SAPCode = ws.Cell(row, 5).GetString().Trim(),
                SAPDescription = ws.Cell(row, 6).GetString().Trim(),
                ITDomain = ws.Cell(row, 7).GetString().Trim(),
                ITSubdomain = ws.Cell(row, 8).GetString().Trim(),
                CommitmentCode = ws.Cell(row, 9).GetString().Trim(),
                HyperionCategory = ws.Cell(row, 10).GetString().Trim(),
                Requestor = ws.Cell(row, 11).GetString().Trim(),
                EPMOId = ws.Cell(row, 12).GetString().Trim(),
                ContractDuration = ws.Cell(row, 13).GetString().Trim(),
                ITComments = ws.Cell(row, 14).GetString().Trim(),
                Jan = ParseDecimal(ws.Cell(row, 15)),
                Feb = ParseDecimal(ws.Cell(row, 16)),
                Mar = ParseDecimal(ws.Cell(row, 17)),
                Apr = ParseDecimal(ws.Cell(row, 18)),
                May = ParseDecimal(ws.Cell(row, 19)),
                Jun = ParseDecimal(ws.Cell(row, 20)),
                Jul = ParseDecimal(ws.Cell(row, 21)),
                Aug = ParseDecimal(ws.Cell(row, 22)),
                Sep = ParseDecimal(ws.Cell(row, 23)),
                Oct = ParseDecimal(ws.Cell(row, 24)),
                Nov = ParseDecimal(ws.Cell(row, 25)),
                Dec = ParseDecimal(ws.Cell(row, 26)),
                CreatedBy = userName,
                CreatedAt = DateTime.UtcNow
            };

            _db.BudgetLines.Add(line);
            imported.Add(line);
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync("BudgetLine", versionId, "Import",
            changedBy: userName, notes: $"Imported {imported.Count} lines");

        return imported;
    }

    private static decimal ParseDecimal(IXLCell cell)
    {
        if (cell.IsEmpty()) return 0;
        if (cell.DataType == XLDataType.Number) return (decimal)cell.GetDouble();
        return decimal.TryParse(cell.GetString(), out var v) ? v : 0;
    }
}
