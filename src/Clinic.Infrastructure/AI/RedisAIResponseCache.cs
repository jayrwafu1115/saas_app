using Clinic.Application.AI;
using StackExchange.Redis;

namespace Clinic.Infrastructure.AI;

public sealed class RedisAIResponseCache(IConnectionMultiplexer redis) : IAIResponseCache
{
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken)
    {
        var value = await redis.GetDatabase().StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken) =>
        redis.GetDatabase().StringSetAsync(key, value, ttl);
}
