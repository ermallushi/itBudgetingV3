using ItBudgetingV3.Domain.Enums;

namespace ItBudgetingV3.Domain.Entities;

public sealed class ChannelTransition
{
    public Guid TransitionId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public ChannelType FromChannel { get; init; }
    public ChannelType ToChannel { get; init; }
    public string TransitionReason { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public DateTimeOffset TransitionTime { get; init; } = DateTimeOffset.UtcNow;
}
