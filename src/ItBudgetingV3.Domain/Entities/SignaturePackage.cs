namespace ItBudgetingV3.Domain.Entities;

public sealed class SignaturePackage
{
    public Guid SignatureId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public string DocumentReference { get; set; } = string.Empty;
    public string SignatureLevel { get; set; } = string.Empty;
    public string SignerAuthMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public string EvidenceReference { get; set; } = string.Empty;
    public DateTimeOffset? SignedAt { get; set; }
}
