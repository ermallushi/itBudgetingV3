using itBudgetingV3.Models;
using itBudgetingV3.Services;
using itBudgetingV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace itBudgetingV3.Controllers;

[Authorize]
public class ProcurementController : Controller
{
    private readonly IProcurementService _procSvc;
    private readonly IBudgetLineService _lineSvc;
    private readonly IBudgetVersionService _versionSvc;

    public ProcurementController(IProcurementService procSvc,
        IBudgetLineService lineSvc, IBudgetVersionService versionSvc)
    {
        _procSvc = procSvc;
        _lineSvc = lineSvc;
        _versionSvc = versionSvc;
    }

    // ── PR ───────────────────────────────────────────────────────────

    // GET /Procurement/PRs/{budgetLineId}
    public async Task<IActionResult> PRs(int budgetLineId)
    {
        var line = await _lineSvc.GetByIdAsync(budgetLineId);
        if (line == null) return NotFound();
        var prs = await _procSvc.GetPRsByBudgetLineAsync(budgetLineId);
        var remaining = await _procSvc.GetTotalRemainingBudgetAsync(budgetLineId);
        ViewBag.Line = line;
        ViewBag.RemainingBudget = remaining;
        return View(prs);
    }

    // GET /Procurement/CreatePR/{budgetLineId}
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> CreatePR(int budgetLineId)
    {
        var line = await _lineSvc.GetByIdAsync(budgetLineId);
        if (line == null) return NotFound();
        var remaining = await _procSvc.GetTotalRemainingBudgetAsync(budgetLineId);

        return View(new PRCreateViewModel
        {
            BudgetLineId = budgetLineId,
            BudgetLineInfo = $"{line.Project} / {line.Vendor}",
            AvailableBudget = remaining,
            PR = new PurchaseRequest { BudgetLineId = budgetLineId }
        });
    }

    // POST /Procurement/CreatePR
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> CreatePR(PRCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var userName = User.Identity?.Name ?? "unknown";
        var pr = await _procSvc.CreatePRAsync(vm.PR, userName);
        TempData["Success"] = $"PR #{pr.PRNumber} created.";
        return RedirectToAction(nameof(PRs), new { budgetLineId = vm.BudgetLineId });
    }

    // POST /Procurement/ApprovePR/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> ApprovePR(int id, int budgetLineId)
    {
        var ok = await _procSvc.ApprovePRAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "PR approved." : "Could not approve PR.";
        return RedirectToAction(nameof(PRs), new { budgetLineId });
    }

    // POST /Procurement/CancelPR/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CancelPR(int id, int budgetLineId)
    {
        var ok = await _procSvc.CancelPRAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "PR cancelled." : "Could not cancel PR.";
        return RedirectToAction(nameof(PRs), new { budgetLineId });
    }

    // ── PO ───────────────────────────────────────────────────────────

    // GET /Procurement/POs/{prId}
    public async Task<IActionResult> POs(int prId)
    {
        var pr = await _procSvc.GetPRByIdAsync(prId);
        if (pr == null) return NotFound();
        var pos = await _procSvc.GetPOsByPRAsync(prId);
        ViewBag.PR = pr;
        return View(pos);
    }

    // GET /Procurement/CreatePO/{prId}
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> CreatePO(int prId)
    {
        var pr = await _procSvc.GetPRByIdAsync(prId);
        if (pr == null) return NotFound();

        return View(new POCreateViewModel
        {
            PurchaseRequestId = prId,
            PRInfo = $"PR#{pr.PRNumber} - {pr.Description}",
            PO = new PurchaseOrder { PurchaseRequestId = prId }
        });
    }

    // POST /Procurement/CreatePO
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> CreatePO(POCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var userName = User.Identity?.Name ?? "unknown";
        var po = await _procSvc.CreatePOAsync(vm.PO, userName);
        TempData["Success"] = $"PO #{po.PONumber} created.";
        return RedirectToAction(nameof(POs), new { prId = vm.PurchaseRequestId });
    }

    // POST /Procurement/UpdatePOUsage
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> UpdatePOUsage(int poId, decimal usedAmount, int prId)
    {
        var po = await _procSvc.GetPOByIdAsync(poId);
        if (po == null) return NotFound();
        po.UsedAmount = usedAmount;
        await _procSvc.UpdatePOAsync(po, User.Identity?.Name ?? "unknown");
        TempData["Success"] = "PO usage updated.";
        return RedirectToAction(nameof(POs), new { prId });
    }

    // POST /Procurement/CancelPO/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CancelPO(int id, int prId)
    {
        var ok = await _procSvc.CancelPOAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "PO cancelled." : "Could not cancel PO.";
        return RedirectToAction(nameof(POs), new { prId });
    }

    // GET /Procurement/Dashboard/{versionId}
    public async Task<IActionResult> Dashboard(int versionId)
    {
        var version = await _versionSvc.GetByIdAsync(versionId);
        if (version == null) return NotFound();
        ViewBag.Version = version;

        var lines = await _lineSvc.GetByVersionAsync(versionId);
        var allPRs = new List<PurchaseRequest>();
        foreach (var line in lines)
            allPRs.AddRange(await _procSvc.GetPRsByBudgetLineAsync(line.Id));

        return View(new ProcurementDashboardViewModel
        {
            PurchaseRequests = allPRs,
            FilterVersionId = versionId
        });
    }
}
