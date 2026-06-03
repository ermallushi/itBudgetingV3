namespace ItBudgetingV3.Domain.Entities;

public sealed class RiskResult
{
    public Guid RiskId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public string SanctionsResult { get; set; } = string.Empty;
    public string PepResult { get; set; } = string.Empty;
    public string AdverseMediaResult { get; set; } = string.Empty;
    public decimal FraudScore { get; set; }
    public decimal FinalRiskScore { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public string ScreeningReference { get; set; } = string.Empty;
}
