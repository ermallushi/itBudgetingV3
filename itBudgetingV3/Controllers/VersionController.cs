using itBudgetingV3.Data;
using itBudgetingV3.Models;
using itBudgetingV3.Services;
using itBudgetingV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Controllers;

[Authorize]
public class VersionController : Controller
{
    private readonly IBudgetVersionService _versionSvc;
    private readonly ApplicationDbContext _db;

    public VersionController(IBudgetVersionService versionSvc, ApplicationDbContext db)
    {
        _versionSvc = versionSvc;
        _db = db;
    }

    // GET /Version
    public async Task<IActionResult> Index()
    {
        var versions = await _versionSvc.GetAllAsync();
        return View(new VersionListViewModel { Versions = versions });
    }

    // GET /Version/Create
    [Authorize(Roles = "Finance,Manager,Admin")]
    public IActionResult Create()
        => View(new VersionCreateViewModel { Year = DateTime.Now.Year });

    // POST /Version/Create
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Finance,Manager,Admin")]
    public async Task<IActionResult> Create(VersionCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (closed, open) = vm.RevisionType switch
        {
            RevisionType.Initial_0_12 => (0, 12),
            RevisionType.Q1_3_9 => (3, 9),
            RevisionType.MidYear_5_7 => (5, 7),
            RevisionType.Final_9_3 => (9, 3),
            _ => (0, 12)
        };

        var label = vm.RevisionType switch
        {
            RevisionType.Initial_0_12 => "0_12",
            RevisionType.Q1_3_9 => "3_9",
            RevisionType.MidYear_5_7 => "5_7",
            RevisionType.Final_9_3 => "9_3",
            _ => "0_12"
        };

        var version = new BudgetVersion
        {
            Name = $"{vm.Year}_PLAN_{label}",
            Year = vm.Year,
            RevisionType = vm.RevisionType,
            ClosedMonths = closed,
            OpenMonths = open,
            Description = vm.Description
        };

        var created = await _versionSvc.CreateAsync(version, User.Identity?.Name ?? "unknown");
        TempData["Success"] = $"Version '{created.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Version/Clone
    [Authorize(Roles = "Finance,Manager,Admin")]
    public async Task<IActionResult> Clone()
    {
        var versions = await _versionSvc.GetAllAsync();
        return View(new VersionCloneViewModel
        {
            AvailableVersions = versions.Where(v =>
                v.Status == BudgetVersionStatus.Approved || v.Status == BudgetVersionStatus.Active)
        });
    }

    // POST /Version/Clone
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Finance,Manager,Admin")]
    public async Task<IActionResult> Clone(VersionCloneViewModel vm)
    {
        try
        {
            var cloned = await _versionSvc.CloneForRevisionAsync(
                vm.SourceVersionId, vm.RevisionType, User.Identity?.Name ?? "unknown");
            TempData["Success"] = $"Version '{cloned.Name}' created from clone.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Clone));
        }
    }

    // POST /Version/Submit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Finance,Manager,Admin")]
    public async Task<IActionResult> Submit(int id)
    {
        var ok = await _versionSvc.SubmitAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "Version submitted." : "Could not submit version.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Version/Approve/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var ok = await _versionSvc.ApproveAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "Version approved." : "Could not approve version.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Version/Activate/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        var ok = await _versionSvc.ActivateAsync(id, User.Identity?.Name ?? "unknown");
        TempData[ok ? "Success" : "Error"] = ok ? "Version activated." : "Could not activate version.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Version/Periods/{id}
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> Periods(int id)
    {
        var version = await _versionSvc.GetByIdAsync(id);
        if (version == null) return NotFound();
        var periods = await _versionSvc.GetPeriodsAsync(id);
        return View(new PeriodManagementViewModel { Version = version, Periods = periods });
    }

    // POST /Version/TogglePeriod
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> TogglePeriod(int versionId, int month, bool isOpen)
    {
        await _versionSvc.TogglePeriodAsync(versionId, month, isOpen, User.Identity?.Name ?? "unknown");
        return RedirectToAction(nameof(Periods), new { id = versionId });
    }

    // GET /Version/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var version = await _versionSvc.GetByIdAsync(id);
        if (version == null) return NotFound();
        return View(version);
    }
}
