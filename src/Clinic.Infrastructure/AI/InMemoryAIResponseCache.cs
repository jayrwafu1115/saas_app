using Clinic.Application.AI;
using Microsoft.Extensions.Caching.Memory;

namespace Clinic.Infrastructure.AI;

public sealed class InMemoryAIResponseCache(IMemoryCache memoryCache) : IAIResponseCache
{
    public Task<string?> GetAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(memoryCache.Get<string>(key));

    public Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        memoryCache.Set(key, value, ttl);
        return Task.CompletedTask;
    }
}
