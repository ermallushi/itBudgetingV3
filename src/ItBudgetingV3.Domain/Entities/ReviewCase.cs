namespace ItBudgetingV3.Domain.Entities;

public sealed class ReviewCase
{
    public Guid ReviewId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public string QueueName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string AssignedUser { get; set; } = string.Empty;
    public string OverrideDecision { get; set; } = string.Empty;
    public List<string> Notes { get; set; } = [];
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
}
