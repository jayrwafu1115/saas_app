namespace Clinic.Application.AI;

public interface IAIResponseCache
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken);
    Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken);
}
