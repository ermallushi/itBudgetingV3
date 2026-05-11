using itBudgetingV3.Models;

namespace itBudgetingV3.Services;

public interface IAuditService
{
    Task LogAsync(string entityName, int entityId, string action, string? fieldName = null,
        string? oldValue = null, string? newValue = null, string? changedBy = null, string? notes = null);
    Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityName = null, int? entityId = null);
}
