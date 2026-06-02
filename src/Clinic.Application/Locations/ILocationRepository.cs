using Clinic.Domain.Tenants;

namespace Clinic.Application.Locations;

public interface ILocationRepository
{
    Task<IReadOnlyList<Location>> ListAsync(Guid? tenantId, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken);
    Task AddAsync(Location location, CancellationToken cancellationToken);
}
