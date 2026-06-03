using ItBudgetingV3.Domain.Entities;

namespace ItBudgetingV3.Domain.Interfaces;

public interface IOnboardingCaseRepository
{
    Task AddAsync(OnboardingCase onboardingCase, CancellationToken cancellationToken = default);
    Task<OnboardingCase?> GetAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<OnboardingCase?> GetByCustomerReferenceAsync(string customerReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OnboardingCase>> ListAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(OnboardingCase onboardingCase, CancellationToken cancellationToken = default);
}
