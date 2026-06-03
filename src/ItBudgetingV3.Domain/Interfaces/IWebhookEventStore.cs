namespace ItBudgetingV3.Domain.Interfaces;

public interface IWebhookEventStore
{
    Task<bool> TryRegisterAsync(string providerEventId, CancellationToken cancellationToken = default);
}
