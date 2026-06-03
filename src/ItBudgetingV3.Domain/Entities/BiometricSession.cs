namespace ItBudgetingV3.Domain.Entities;

public sealed class BiometricSession
{
    public Guid BiometricId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public string SelfieFileReference { get; set; } = string.Empty;
    public string LivenessResult { get; set; } = string.Empty;
    public decimal FaceMatchScore { get; set; }
    public string ProviderReference { get; set; } = string.Empty;
    public string DecisionReason { get; set; } = string.Empty;
}
