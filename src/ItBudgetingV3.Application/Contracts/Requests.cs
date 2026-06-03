using ItBudgetingV3.Domain.Enums;

namespace ItBudgetingV3.Application.Contracts;

public sealed record CreateCaseRequest(string ExternalReference, JourneyType JourneyType, ChannelType CurrentChannel, string CustomerReference);
public sealed record PersonProfileRequest(string FirstName, string LastName, DateOnly? DateOfBirth, string Nationality, string Email, string Phone, string Address, Dictionary<string, bool>? ConsentFlags);
public sealed record IdentityDocumentRequest(string DocumentType, string IssuingCountry, string DocumentNumber, DateOnly? ExpiryDate, string FrontFileReference, string BackFileReference, string NfcReference);
public sealed record DocumentVerificationCallbackRequest(Guid DocumentId, bool IsSuccessful, string AuthenticityResult, string TamperResult, string ExtractionPayload, string ProviderEventId, string CorrelationId);
public sealed record BiometricSessionRequest(string SelfieFileReference);
public sealed record BiometricCallbackRequest(Guid BiometricId, bool IsSuccessful, string LivenessResult, decimal FaceMatchScore, string DecisionReason, string ProviderEventId, string CorrelationId);
public sealed record RiskScreeningRequest(decimal FraudScore, string SanctionsResult, string PepResult, string AdverseMediaResult);
public sealed record ReviewRequest(string QueueName, string Reason, string AssignedUser);
public sealed record ReviewDecisionRequest(string Decision, string Note, string Reviewer);
public sealed record SignaturePackageRequest(string DocumentReference, string SignatureLevel, string SignerAuthMethod);
public sealed record SignatureCallbackRequest(Guid SignatureId, bool IsSigned, string EvidenceReference, string ProviderEventId, string CorrelationId);
public sealed record ChannelTransitionRequest(ChannelType ToChannel, string Reason, string PerformedBy);
