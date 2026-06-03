using ItBudgetingV3.Domain.Enums;

namespace ItBudgetingV3.Domain.Entities;

public sealed class AuditEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public ActorType ActorType { get; init; }
    public string ActorId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public DateTimeOffset EventTime { get; init; } = DateTimeOffset.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
    public string PayloadReference { get; init; } = string.Empty;
}
