using Clinic.Domain.AI;

namespace Clinic.Application.AI;

public static class AIMapping
{
    public static AIGenerationDto ToDto(this AIGeneration generation) =>
        new(
            generation.Id,
            generation.TenantId,
            generation.EncounterId,
            generation.Type,
            generation.Provider,
            generation.Model,
            generation.Status,
            generation.Output,
            generation.ErrorMessage,
            generation.PromptTokens,
            generation.CompletionTokens,
            generation.TotalTokens,
            generation.CostUsd,
            generation.LatencyMs,
            generation.ServedFromCache,
            generation.CompletedAtUtc);
}
