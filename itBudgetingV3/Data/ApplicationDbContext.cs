using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using itBudgetingV3.Models;

namespace itBudgetingV3.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BudgetVersion> BudgetVersions { get; set; }
    public DbSet<BudgetLine> BudgetLines { get; set; }
    public DbSet<BudgetPeriod> BudgetPeriods { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<BudgetTransfer> BudgetTransfers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed cost centers
        builder.Entity<CostCenter>().HasData(
            new CostCenter { Code = "91I40000", Name = "BSS/OSS Operation" },
            new CostCenter { Code = "91I20000", Name = "BSS/OSS DEV" },
            new CostCenter { Code = "91I10000", Name = "Infrastructure" },
            new CostCenter { Code = "91I50000", Name = "EPM and IT Governance" },
            new CostCenter { Code = "91I00000", Name = "CIO Office" }
        );

        // Seed roles
        var adminRoleId = "a1b2c3d4-0001-0000-0000-000000000001";
        var managerRoleId = "a1b2c3d4-0001-0000-0000-000000000002";
        var financeRoleId = "a1b2c3d4-0001-0000-0000-000000000003";
        var userRoleId = "a1b2c3d4-0001-0000-0000-000000000004";

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = adminRoleId },
            new IdentityRole { Id = managerRoleId, Name = "Manager", NormalizedName = "MANAGER", ConcurrencyStamp = managerRoleId },
            new IdentityRole { Id = financeRoleId, Name = "Finance", NormalizedName = "FINANCE", ConcurrencyStamp = financeRoleId },
            new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER", ConcurrencyStamp = userRoleId }
        );

        // Seed admin user
        var adminUserId = "a1b2c3d4-0002-0000-0000-000000000001";
        var hasher = new PasswordHasher<ApplicationUser>();
        var adminUser = new ApplicationUser
        {
            Id = adminUserId,
            UserName = "admin@itbudgeting.com",
            NormalizedUserName = "ADMIN@ITBUDGETING.COM",
            Email = "admin@itbudgeting.com",
            NormalizedEmail = "ADMIN@ITBUDGETING.COM",
            EmailConfirmed = true,
            FullName = "System Administrator",
            SecurityStamp = "a1b2c3d4-0003-0000-0000-000000000001",
            ConcurrencyStamp = "a1b2c3d4-0003-0000-0000-000000000002"
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123!");

        builder.Entity<ApplicationUser>().HasData(adminUser);
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId }
        );

        // BudgetLine: ForecastTotal is computed at application level ([NotMapped])
        // PurchaseOrder: RemainingAmount is computed at application level ([NotMapped])
        // No DB computed column config needed for SQLite
    }
}

