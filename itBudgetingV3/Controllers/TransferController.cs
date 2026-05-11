using itBudgetingV3.Models;
using itBudgetingV3.Services;
using itBudgetingV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace itBudgetingV3.Controllers;

[Authorize]
public class TransferController : Controller
{
    private readonly ITransferService _transferSvc;
    private readonly IBudgetVersionService _versionSvc;
    private readonly IBudgetLineService _lineSvc;

    public TransferController(ITransferService transferSvc,
        IBudgetVersionService versionSvc, IBudgetLineService lineSvc)
    {
        _transferSvc = transferSvc;
        _versionSvc = versionSvc;
        _lineSvc = lineSvc;
    }

    // GET /Transfer/Index/{versionId}
    public async Task<IActionResult> Index(int versionId)
    {
        var version = await _versionSvc.GetByIdAsync(versionId);
        if (version == null) return NotFound();
        var transfers = await _transferSvc.GetByVersionAsync(versionId);
        return View(new TransferListViewModel { Version = version, Transfers = transfers });
    }

    // GET /Transfer/Create/{versionId}
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> Create(int versionId)
    {
        var version = await _versionSvc.GetByIdAsync(versionId);
        if (version == null) return NotFound();
        var lines = await _lineSvc.GetByVersionAsync(versionId);
        var periods = await _versionSvc.GetPeriodsAsync(versionId);

        return View(new TransferCreateViewModel
        {
            BudgetVersionId = versionId,
            BudgetLines = lines,
            Periods = periods.Where(p => p.IsOpen),
            Transfer = new BudgetTransfer { BudgetVersionId = versionId }
        });
    }

    // POST /Transfer/Create
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> Create(TransferCreateViewModel vm)
    {
        var userName = User.Identity?.Name ?? "unknown";
        try
        {
            var transfer = await _transferSvc.CreateTransferAsync(vm.Transfer, userName);
            TempData["Success"] = transfer.IsCrossCategory
                ? "Cross-category transfer submitted for approval."
                : "Transfer applied successfully.";
            return RedirectToAction(nameof(Index), new { versionId = vm.BudgetVersionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create), new { versionId = vm.BudgetVersionId });
        }
    }

    // POST /Transfer/Approve/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Approve(int id, int versionId)
    {
        var ok = await _transferSvc.ApproveTransferAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "Transfer approved." : "Could not approve transfer.";
        return RedirectToAction(nameof(Index), new { versionId });
    }

    // POST /Transfer/Reject/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Reject(int id, int versionId)
    {
        var ok = await _transferSvc.RejectTransferAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "Transfer rejected." : "Could not reject transfer.";
        return RedirectToAction(nameof(Index), new { versionId });
    }
}
