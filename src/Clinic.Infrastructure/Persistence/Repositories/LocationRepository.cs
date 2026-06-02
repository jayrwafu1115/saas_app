using Clinic.Application.Locations;
using Clinic.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class LocationRepository(ApplicationDbContext dbContext) : ILocationRepository
{
    public async Task<IReadOnlyList<Location>> ListAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var query = dbContext.Locations.AsNoTracking();
        if (tenantId.HasValue)
        {
            query = query.Where(location => location.TenantId == tenantId.Value);
        }

        return await query
            .OrderBy(location => location.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return dbContext.Locations.AnyAsync(
            location => location.TenantId == tenantId && location.Code == normalizedCode,
            cancellationToken);
    }

    public async Task AddAsync(Location location, CancellationToken cancellationToken)
    {
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
