using System.ComponentModel.DataAnnotations;

namespace itBudgetingV3.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;  // Create / Update / Delete / Approve / Transfer

    [MaxLength(100)]
    public string? FieldName { get; set; }

    [MaxLength(2000)]
    public string? OldValue { get; set; }

    [MaxLength(2000)]
    public string? NewValue { get; set; }

    [MaxLength(256)]
    public string? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
