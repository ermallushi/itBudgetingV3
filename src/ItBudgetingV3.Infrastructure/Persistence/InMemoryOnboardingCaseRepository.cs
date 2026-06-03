using System.Collections.Concurrent;
using ItBudgetingV3.Domain.Entities;
using ItBudgetingV3.Domain.Interfaces;

namespace ItBudgetingV3.Infrastructure.Persistence;

public sealed class InMemoryOnboardingCaseRepository : IOnboardingCaseRepository
{
    private readonly ConcurrentDictionary<Guid, OnboardingCase> _cases = new();

    public Task AddAsync(OnboardingCase onboardingCase, CancellationToken cancellationToken = default)
    {
        _cases[onboardingCase.CaseId] = onboardingCase;
        return Task.CompletedTask;
    }

    public Task<OnboardingCase?> GetAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        _cases.TryGetValue(caseId, out var onboardingCase);
        return Task.FromResult(onboardingCase);
    }

    public Task<OnboardingCase?> GetByCustomerReferenceAsync(string customerReference, CancellationToken cancellationToken = default)
    {
        var onboardingCase = _cases.Values.FirstOrDefault(c => string.Equals(c.CustomerReference, customerReference, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(onboardingCase);
    }

    public Task<IReadOnlyCollection<OnboardingCase>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyCollection<OnboardingCase>)_cases.Values.ToArray());

    public Task UpdateAsync(OnboardingCase onboardingCase, CancellationToken cancellationToken = default)
    {
        _cases[onboardingCase.CaseId] = onboardingCase;
        return Task.CompletedTask;
    }
}
