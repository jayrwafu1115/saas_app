using System.Text.Json;
using Clinic.Application.Reporting;
using StackExchange.Redis;

namespace Clinic.Infrastructure.Reporting;

public sealed class RedisReportingCache(IConnectionMultiplexer redis) : IReportingCache
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var value = await redis.GetDatabase().StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken) =>
        redis.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value), ttl);
}
