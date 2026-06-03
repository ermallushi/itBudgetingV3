namespace ItBudgetingV3.Domain.Entities;

public sealed class IdentityDocument
{
    public Guid DocumentId { get; init; } = Guid.NewGuid();
    public Guid CaseId { get; init; }
    public string DocumentType { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public string FrontFileReference { get; set; } = string.Empty;
    public string BackFileReference { get; set; } = string.Empty;
    public string NfcReference { get; set; } = string.Empty;
    public string ExtractionPayload { get; set; } = string.Empty;
    public string AuthenticityResult { get; set; } = string.Empty;
    public string TamperResult { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
}
