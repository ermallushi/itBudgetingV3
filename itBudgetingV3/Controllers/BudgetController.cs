using itBudgetingV3.Data;
using itBudgetingV3.Models;
using itBudgetingV3.Services;
using itBudgetingV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Controllers;

[Authorize]
public class BudgetController : Controller
{
    private readonly IBudgetVersionService _versionSvc;
    private readonly IBudgetLineService _lineSvc;
    private readonly ApplicationDbContext _db;

    public BudgetController(IBudgetVersionService versionSvc, IBudgetLineService lineSvc, ApplicationDbContext db)
    {
        _versionSvc = versionSvc;
        _lineSvc = lineSvc;
        _db = db;
    }

    // GET /Budget/Grid/{versionId}
    public async Task<IActionResult> Grid(int versionId, string? costCenter, BudgetCategory? category)
    {
        var version = await _versionSvc.GetByIdAsync(versionId);
        if (version == null) return NotFound();

        var lines = await _lineSvc.GetByVersionAsync(versionId, costCenter, category);
        var periods = await _versionSvc.GetPeriodsAsync(versionId);
        var (total, committed, remaining) = await _lineSvc.GetBudgetSummaryAsync(versionId);
        var costCenters = await _db.CostCenters.Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();

        var vm = new BudgetGridViewModel
        {
            Version = version,
            Lines = lines,
            Periods = periods.ToList(),
            CostCenters = costCenters,
            TotalBudget = total,
            TotalCommitted = committed,
            TotalRemaining = remaining,
            FilterCostCenter = costCenter,
            FilterCategory = category
        };

        return View(vm);
    }

    // GET /Budget/Lines/{versionId} — returns JSON for AG Grid
    [HttpGet]
    public async Task<IActionResult> Lines(int versionId, string? costCenter, int? category)
    {
        var lines = await _lineSvc.GetByVersionAsync(versionId, costCenter,
            category.HasValue ? (BudgetCategory)category.Value : null);

        var data = lines.Select(l => new
        {
            l.Id, l.WBS, l.Project, l.Vendor,
            Category = l.Category.ToString(),
            l.CostCenterCode,
            CostCenterName = l.CostCenter?.Name,
            l.SAPCode, l.SAPDescription, l.ITDomain, l.ITSubdomain,
            l.CommitmentCode, l.HyperionCategory,
            l.Requestor, l.EPMOId, l.ContractDuration, l.ITComments,
            l.Jan, l.Feb, l.Mar, l.Apr, l.May, l.Jun,
            l.Jul, l.Aug, l.Sep, l.Oct, l.Nov, l.Dec,
            ForecastTotal = l.Jan + l.Feb + l.Mar + l.Apr + l.May + l.Jun +
                            l.Jul + l.Aug + l.Sep + l.Oct + l.Nov + l.Dec
        });

        return Json(data);
    }

    // POST /Budget/SaveLine — saves a single row from AG Grid
    [HttpPost]
    [Authorize(Roles = "User,Manager,Finance,Admin")]
    public async Task<IActionResult> SaveLine([FromBody] BudgetLineSaveDto dto)
    {
        var userName = User.Identity?.Name ?? "unknown";

        var line = new BudgetLine
        {
            Id = dto.Id,
            BudgetVersionId = dto.BudgetVersionId,
            WBS = dto.WBS,
            Project = dto.Project,
            Vendor = dto.Vendor,
            Category = (BudgetCategory)dto.Category,
            CostCenterCode = dto.CostCenterCode,
            SAPCode = dto.SAPCode,
            SAPDescription = dto.SAPDescription,
            ITDomain = dto.ITDomain,
            ITSubdomain = dto.ITSubdomain,
            CommitmentCode = dto.CommitmentCode,
            HyperionCategory = dto.HyperionCategory,
            Requestor = dto.Requestor,
            EPMOId = dto.EPMOId,
            ContractDuration = dto.ContractDuration,
            ITComments = dto.ITComments,
            Jan = dto.Jan, Feb = dto.Feb, Mar = dto.Mar, Apr = dto.Apr,
            May = dto.May, Jun = dto.Jun, Jul = dto.Jul, Aug = dto.Aug,
            Sep = dto.Sep, Oct = dto.Oct, Nov = dto.Nov, Dec = dto.Dec
        };

        try
        {
            BudgetLine saved;
            if (dto.Id == 0)
                saved = await _lineSvc.CreateAsync(line, userName);
            else
                saved = await _lineSvc.UpdateAsync(line, userName);

            return Json(new { success = true, id = saved.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // DELETE /Budget/DeleteLine/{id}
    [HttpDelete]
    [Authorize(Roles = "User,Manager,Finance,Admin")]
    public async Task<IActionResult> DeleteLine(int id)
    {
        var userName = User.Identity?.Name ?? "unknown";
        var ok = await _lineSvc.DeleteAsync(id, userName);
        return Json(new { success = ok });
    }

    // GET /Budget/Import/{versionId}
    [HttpGet]
    [Authorize(Roles = "User,Manager,Finance,Admin")]
    public async Task<IActionResult> Import(int versionId)
    {
        var version = await _versionSvc.GetByIdAsync(versionId);
        if (version == null) return NotFound();
        return View(version);
    }

    // POST /Budget/Import
    [HttpPost]
    [Authorize(Roles = "User,Manager,Finance,Admin")]
    public async Task<IActionResult> Import(int versionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an Excel file.";
            return RedirectToAction(nameof(Import), new { versionId });
        }

        var userName = User.Identity?.Name ?? "unknown";
        try
        {
            using var stream = file.OpenReadStream();
            var imported = await _lineSvc.ImportFromExcelAsync(versionId, stream, userName);
            TempData["Success"] = $"Successfully imported {imported.Count()} budget lines.";
            return RedirectToAction(nameof(Grid), new { versionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Import failed: {ex.Message}";
            return RedirectToAction(nameof(Import), new { versionId });
        }
    }

    // GET /Budget/ExportTemplate
    [HttpGet]
    public IActionResult ExportTemplate()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("CapEx");
        var headers = new[] { "WBS", "Project", "Vendor", "CostCenter", "SAPCode", "SAPDescription",
            "ITDomain", "ITSubdomain", "CommitmentCode", "HyperionCategory",
            "Requestor", "EPMOId", "ContractDuration", "ITComments",
            "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        ws.Row(1).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "BudgetImportTemplate.xlsx");
    }
}
