# itBudgetingV3

Modular .NET 8 onboarding platform foundation for digital, assisted, and cross-channel customer onboarding journeys.

## Solution structure

- `/src/ItBudgetingV3.Domain` - core entities, enums, state model, and repository contracts
- `/src/ItBudgetingV3.Application` - orchestration service, request/response contracts, provider abstractions
- `/src/ItBudgetingV3.Infrastructure` - in-memory persistence, idempotent webhook store, stub provider adapters, DI wiring
- `/src/ItBudgetingV3.Api` - REST API for case lifecycle, identity, biometrics, risk, review, signature, store, webhook, and reporting endpoints
- `/tests/ItBudgetingV3.Tests` - xUnit coverage for lifecycle, idempotency, invalid transitions, and cross-channel continuity

## Implemented foundation

This repository now includes the foundation phase and a runnable onboarding orchestration API with:

- a single onboarding case as the orchestration record
- explicit case status transitions with audit events
- channel transition history for digital-to-store continuation
- adapter boundaries for identity, biometric, risk, and signature providers
- idempotent webhook processing for external callbacks
- review queue support and operational metrics endpoint

## Run

```bash
dotnet build ItBudgetingV3.slnx
dotnet test ItBudgetingV3.slnx
dotnet run --project src/ItBudgetingV3.Api/ItBudgetingV3.Api.csproj
```

## Notes

- Persistence is currently in-memory to keep the platform modular and easy to extend.
- Provider integrations are stubbed behind interfaces so real vendors can be added with minimal impact.
- Security, external systems, and UI channels should be implemented in later phases on top of this foundation.
