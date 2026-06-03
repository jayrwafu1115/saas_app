using Clinic.Domain.AI;

namespace Clinic.Application.AI;

public interface IAIService
{
    Task<AIGenerationDto> QueueGenerationAsync(Guid encounterId, AIGenerationType type, string provider, string? model, CancellationToken cancellationToken);
    Task ProcessGenerationAsync(Guid generationId, CancellationToken cancellationToken);
}
