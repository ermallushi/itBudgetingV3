using System.Collections.Concurrent;
using ItBudgetingV3.Domain.Interfaces;

namespace ItBudgetingV3.Infrastructure.Persistence;

public sealed class InMemoryWebhookEventStore : IWebhookEventStore
{
    private readonly ConcurrentDictionary<string, byte> _processedEvents = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> TryRegisterAsync(string providerEventId, CancellationToken cancellationToken = default)
        => Task.FromResult(_processedEvents.TryAdd(providerEventId, 0));
}
