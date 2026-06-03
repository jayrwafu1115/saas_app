using Clinic.Application.AI;
using Clinic.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class AIGenerationRepository(ApplicationDbContext dbContext) : IAIGenerationRepository
{
    public Task<AIGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AIGenerations.FirstOrDefaultAsync(generation => generation.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AIGeneration>> ListByEncounterAsync(Guid encounterId, CancellationToken cancellationToken) =>
        await dbContext.AIGenerations
            .AsNoTracking()
            .Where(generation => generation.EncounterId == encounterId)
            .OrderByDescending(generation => generation.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<AIUsageSummaryDto> GetUsageAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var query = dbContext.AIGenerations.AsNoTracking().Where(generation => generation.TenantId == tenantId);
        var items = await query.ToListAsync(cancellationToken);
        return new AIUsageSummaryDto(
            tenantId,
            items.Count,
            items.Count(item => item.Status == AIGenerationStatus.Completed),
            items.Count(item => item.Status == AIGenerationStatus.Failed),
            items.Sum(item => item.TotalTokens),
            items.Sum(item => item.CostUsd),
            items.Count == 0 ? 0 : items.Average(item => item.LatencyMs));
    }

    public async Task AddAsync(AIGeneration generation, CancellationToken cancellationToken)
    {
        dbContext.AIGenerations.Add(generation);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
