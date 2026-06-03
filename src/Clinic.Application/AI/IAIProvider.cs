using Clinic.Domain.AI;

namespace Clinic.Application.AI;

public interface IAIProvider
{
    string Name { get; }
    Task<AIProviderResponse> GenerateAsync(AIGenerationType type, string prompt, string? model, CancellationToken cancellationToken);
}
