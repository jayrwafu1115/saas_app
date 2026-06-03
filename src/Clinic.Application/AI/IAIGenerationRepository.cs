using Clinic.Domain.AI;

namespace Clinic.Application.AI;

public interface IAIGenerationRepository
{
    Task<AIGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AIGeneration>> ListByEncounterAsync(Guid encounterId, CancellationToken cancellationToken);
    Task<AIUsageSummaryDto> GetUsageAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddAsync(AIGeneration generation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
