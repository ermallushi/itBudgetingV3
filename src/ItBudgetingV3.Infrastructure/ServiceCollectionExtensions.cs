using ItBudgetingV3.Application.Providers;
using ItBudgetingV3.Application.Services;
using ItBudgetingV3.Domain.Interfaces;
using ItBudgetingV3.Infrastructure.Persistence;
using ItBudgetingV3.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ItBudgetingV3.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOnboardingPlatform(this IServiceCollection services)
    {
        services.AddSingleton<IOnboardingCaseRepository, InMemoryOnboardingCaseRepository>();
        services.AddSingleton<IWebhookEventStore, InMemoryWebhookEventStore>();
        services.AddSingleton<IIdentityVerificationProvider, StubIdentityVerificationProvider>();
        services.AddSingleton<IBiometricVerificationProvider, StubBiometricVerificationProvider>();
        services.AddSingleton<IRiskScreeningProvider, StubRiskScreeningProvider>();
        services.AddSingleton<IDigitalSignatureProvider, StubDigitalSignatureProvider>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        return services;
    }
}
