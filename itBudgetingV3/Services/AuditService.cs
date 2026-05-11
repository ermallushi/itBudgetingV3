using itBudgetingV3.Data;
using itBudgetingV3.Models;
using Microsoft.EntityFrameworkCore;

namespace itBudgetingV3.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db) => _db = db;

    public async Task LogAsync(string entityName, int entityId, string action,
        string? fieldName = null, string? oldValue = null, string? newValue = null,
        string? changedBy = null, string? notes = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            Notes = notes,
            ChangedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityName = null, int? entityId = null)
    {
        var q = _db.AuditLogs.AsQueryable();
        if (entityName != null) q = q.Where(a => a.EntityName == entityName);
        if (entityId.HasValue) q = q.Where(a => a.EntityId == entityId.Value);
        return await q.OrderByDescending(a => a.ChangedAt).ToListAsync();
    }
}
