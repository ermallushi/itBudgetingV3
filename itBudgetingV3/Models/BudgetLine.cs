using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace itBudgetingV3.Models;

public enum BudgetCategory
{
    CapEx = 0,
    OpEx = 1
}

public class BudgetLine
{
    public int Id { get; set; }

    [Required]
    public int BudgetVersionId { get; set; }
    public BudgetVersion BudgetVersion { get; set; } = null!;

    // Core fields
    [MaxLength(50)]
    public string? WBS { get; set; }

    [MaxLength(200)]
    public string? Project { get; set; }

    [MaxLength(200)]
    public string? Vendor { get; set; }

    public BudgetCategory Category { get; set; }

    [Required, MaxLength(20)]
    public string CostCenterCode { get; set; } = string.Empty;

    public CostCenter? CostCenter { get; set; }

    // SAP / Financial
    [MaxLength(50)]
    public string? SAPCode { get; set; }

    [MaxLength(200)]
    public string? SAPDescription { get; set; }

    [MaxLength(100)]
    public string? ITDomain { get; set; }

    [MaxLength(100)]
    public string? ITSubdomain { get; set; }

    [MaxLength(50)]
    public string? CommitmentCode { get; set; }

    [MaxLength(100)]
    public string? HyperionCategory { get; set; }

    // Additional
    [MaxLength(100)]
    public string? Requestor { get; set; }

    [MaxLength(50)]
    public string? EPMOId { get; set; }

    [MaxLength(100)]
    public string? ContractDuration { get; set; }  // OpEx specific

    [MaxLength(500)]
    public string? ITComments { get; set; }

    // Monthly planning (EUR)
    [Column(TypeName = "decimal(18,2)")]
    public decimal Jan { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Feb { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Mar { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Apr { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal May { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Jun { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Jul { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Aug { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Sep { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Oct { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Nov { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Dec { get; set; }

    [NotMapped]
    public decimal ForecastTotal => Jan + Feb + Mar + Apr + May + Jun + Jul + Aug + Sep + Oct + Nov + Dec;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    // Navigation
    public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();

    // Helper: get value for a given month number (1=Jan, 12=Dec)
    public decimal GetMonth(int month) => month switch
    {
        1 => Jan, 2 => Feb, 3 => Mar, 4 => Apr,
        5 => May, 6 => Jun, 7 => Jul, 8 => Aug,
        9 => Sep, 10 => Oct, 11 => Nov, 12 => Dec,
        _ => 0
    };

    public void SetMonth(int month, decimal value)
    {
        switch (month)
        {
            case 1: Jan = value; break;
            case 2: Feb = value; break;
            case 3: Mar = value; break;
            case 4: Apr = value; break;
            case 5: May = value; break;
            case 6: Jun = value; break;
            case 7: Jul = value; break;
            case 8: Aug = value; break;
            case 9: Sep = value; break;
            case 10: Oct = value; break;
            case 11: Nov = value; break;
            case 12: Dec = value; break;
        }
    }
}
