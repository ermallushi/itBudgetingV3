using ItBudgetingV3.Domain.Entities;
using ItBudgetingV3.Domain.Enums;

namespace ItBudgetingV3.Application.Contracts;

public sealed record CaseSummaryResponse(
    Guid CaseId,
    string ExternalReference,
    JourneyType JourneyType,
    ChannelType CurrentChannel,
    ChannelType? PreviousChannel,
    CaseStatus Status,
    string SubStatus,
    string CustomerReference,
    string AssignedQueue,
    string RiskLevel,
    FinalDecision FinalDecision,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    PersonProfile? PersonProfile,
    IReadOnlyCollection<IdentityDocument> IdentityDocuments,
    IReadOnlyCollection<BiometricSession> BiometricSessions,
    IReadOnlyCollection<RiskResult> RiskResults,
    IReadOnlyCollection<ReviewCase> ReviewCases,
    IReadOnlyCollection<SignaturePackage> SignaturePackages,
    IReadOnlyCollection<AuditEvent> AuditEvents,
    IReadOnlyCollection<ChannelTransition> ChannelTransitions);

public sealed record OperationalMetricsResponse(
    int TotalCases,
    int CompletedCases,
    int RejectedCases,
    int ManualReviewCases,
    IReadOnlyDictionary<CaseStatus, int> CasesByStatus);
