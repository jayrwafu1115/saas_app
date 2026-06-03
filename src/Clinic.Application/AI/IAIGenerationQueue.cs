namespace Clinic.Application.AI;

public interface IAIGenerationQueue
{
    ValueTask EnqueueAsync(Guid generationId, CancellationToken cancellationToken);
    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
