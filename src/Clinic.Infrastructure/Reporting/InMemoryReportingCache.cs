using System.Text.Json;
using Clinic.Application.Reporting;
using Microsoft.Extensions.Caching.Memory;

namespace Clinic.Infrastructure.Reporting;

public sealed class InMemoryReportingCache(IMemoryCache memoryCache) : IReportingCache
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var json = memoryCache.Get<string>(key);
        return Task.FromResult(json is null ? default : JsonSerializer.Deserialize<T>(json));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        memoryCache.Set(key, JsonSerializer.Serialize(value), ttl);
        return Task.CompletedTask;
    }
}
