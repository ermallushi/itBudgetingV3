using itBudgetingV3.Models;
using itBudgetingV3.Services;
using itBudgetingV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace itBudgetingV3.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly IBudgetVersionService _versionSvc;
    private readonly IBudgetLineService _lineSvc;
    private readonly IProcurementService _procSvc;
    private readonly IAuditService _auditSvc;

    public ReportController(IBudgetVersionService versionSvc, IBudgetLineService lineSvc,
        IProcurementService procSvc, IAuditService auditSvc)
    {
        _versionSvc = versionSvc;
        _lineSvc = lineSvc;
        _procSvc = procSvc;
        _auditSvc = auditSvc;
    }

    // GET /Report
    public async Task<IActionResult> Index(int? versionId)
    {
        var versions = await _versionSvc.GetAllAsync();
        var vm = new ReportViewModel { Versions = versions, SelectedVersionId = versionId };

        if (versionId.HasValue)
        {
            var lines = (await _lineSvc.GetByVersionAsync(versionId.Value)).ToList();
            var rows = new List<ReportRow>();

            foreach (var line in lines)
            {
                var committed = await _procSvc.GetTotalCommittedAsync(line.Id);
                var totalBudget = line.Jan + line.Feb + line.Mar + line.Apr + line.May + line.Jun +
                                  line.Jul + line.Aug + line.Sep + line.Oct + line.Nov + line.Dec;
                rows.Add(new ReportRow
                {
                    Project = line.Project,
                    Vendor = line.Vendor,
                    CostCenter = line.CostCenterCode,
                    Category = line.Category.ToString(),
                    TotalBudget = totalBudget,
                    TotalCommitted = committed,
                    TotalRemaining = totalBudget - committed
                });
            }

            vm.Rows = rows;
            vm.TotalBudget = rows.Sum(r => r.TotalBudget);
            vm.TotalCommitted = rows.Sum(r => r.TotalCommitted);
            vm.TotalRemaining = rows.Sum(r => r.TotalRemaining);
            vm.TotalCapEx = rows.Where(r => r.Category == "CapEx").Sum(r => r.TotalBudget);
            vm.TotalOpEx = rows.Where(r => r.Category == "OpEx").Sum(r => r.TotalBudget);
        }

        return View(vm);
    }

    // GET /Report/VersionComparison
    public async Task<IActionResult> VersionComparison(int? v1, int? v2)
    {
        var versions = await _versionSvc.GetAllAsync();
        ViewBag.Versions = versions;

        if (v1.HasValue && v2.HasValue)
        {
            var lines1 = (await _lineSvc.GetByVersionAsync(v1.Value)).ToList();
            var lines2 = (await _lineSvc.GetByVersionAsync(v2.Value)).ToList();

            var ver1 = await _versionSvc.GetByIdAsync(v1.Value);
            var ver2 = await _versionSvc.GetByIdAsync(v2.Value);

            ViewBag.Version1 = ver1;
            ViewBag.Version2 = ver2;
            ViewBag.Total1 = lines1.Sum(l => l.Jan + l.Feb + l.Mar + l.Apr + l.May + l.Jun +
                                             l.Jul + l.Aug + l.Sep + l.Oct + l.Nov + l.Dec);
            ViewBag.Total2 = lines2.Sum(l => l.Jan + l.Feb + l.Mar + l.Apr + l.May + l.Jun +
                                             l.Jul + l.Aug + l.Sep + l.Oct + l.Nov + l.Dec);
            ViewBag.Lines1 = lines1;
            ViewBag.Lines2 = lines2;
        }

        return View();
    }

    // GET /Report/Audit
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> Audit(string? entity)
    {
        var logs = await _auditSvc.GetLogsAsync(entity);
        ViewBag.EntityFilter = entity;
        return View(logs);
    }

    // GET /Report/Export/{versionId}
    public async Task<IActionResult> Export(int versionId)
    {
        var lines = (await _lineSvc.GetByVersionAsync(versionId)).ToList();
        var version = await _versionSvc.GetByIdAsync(versionId);

        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.Worksheets.Add("Budget");
        var headers = new[] { "WBS", "Project", "Vendor", "Category", "CostCenter",
            "SAPCode", "SAPDescription", "ITDomain", "ITSubdomain", "CommitmentCode",
            "HyperionCategory", "Requestor", "EPMOId", "ContractDuration", "ITComments",
            "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec","Total" };

        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var l in lines)
        {
            ws.Cell(row, 1).Value = l.WBS;
            ws.Cell(row, 2).Value = l.Project;
            ws.Cell(row, 3).Value = l.Vendor;
            ws.Cell(row, 4).Value = l.Category.ToString();
            ws.Cell(row, 5).Value = l.CostCenterCode;
            ws.Cell(row, 6).Value = l.SAPCode;
            ws.Cell(row, 7).Value = l.SAPDescription;
            ws.Cell(row, 8).Value = l.ITDomain;
            ws.Cell(row, 9).Value = l.ITSubdomain;
            ws.Cell(row, 10).Value = l.CommitmentCode;
            ws.Cell(row, 11).Value = l.HyperionCategory;
            ws.Cell(row, 12).Value = l.Requestor;
            ws.Cell(row, 13).Value = l.EPMOId;
            ws.Cell(row, 14).Value = l.ContractDuration;
            ws.Cell(row, 15).Value = l.ITComments;
            ws.Cell(row, 16).Value = (double)l.Jan;
            ws.Cell(row, 17).Value = (double)l.Feb;
            ws.Cell(row, 18).Value = (double)l.Mar;
            ws.Cell(row, 19).Value = (double)l.Apr;
            ws.Cell(row, 20).Value = (double)l.May;
            ws.Cell(row, 21).Value = (double)l.Jun;
            ws.Cell(row, 22).Value = (double)l.Jul;
            ws.Cell(row, 23).Value = (double)l.Aug;
            ws.Cell(row, 24).Value = (double)l.Sep;
            ws.Cell(row, 25).Value = (double)l.Oct;
            ws.Cell(row, 26).Value = (double)l.Nov;
            ws.Cell(row, 27).Value = (double)l.Dec;
            ws.Cell(row, 28).Value = (double)(l.Jan + l.Feb + l.Mar + l.Apr + l.May + l.Jun +
                                               l.Jul + l.Aug + l.Sep + l.Oct + l.Nov + l.Dec);
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Budget_{version?.Name ?? versionId.ToString()}.xlsx");
    }
}
