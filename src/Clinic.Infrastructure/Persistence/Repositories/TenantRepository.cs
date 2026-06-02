using Clinic.Application.Tenants;
using Clinic.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(ApplicationDbContext dbContext) : ITenantRepository
{
    public async Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Tenants
            .AsNoTracking()
            .OrderBy(tenant => tenant.Name)
            .ToListAsync(cancellationToken);

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tenants.FirstOrDefaultAsync(tenant => tenant.Id == id, cancellationToken);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return dbContext.Tenants.FirstOrDefaultAsync(tenant => tenant.Slug == normalizedSlug, cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return dbContext.Tenants.AnyAsync(tenant => tenant.Slug == normalizedSlug, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
