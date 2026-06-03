using Clinic.Domain.AI;

namespace Clinic.Application.AI;

public sealed record AIGenerationDto(
    Guid Id,
    Guid TenantId,
    Guid EncounterId,
    AIGenerationType Type,
    string Provider,
    string Model,
    AIGenerationStatus Status,
    string Output,
    string ErrorMessage,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    decimal CostUsd,
    long LatencyMs,
    bool ServedFromCache,
    DateTimeOffset? CompletedAtUtc);

public sealed record AIUsageSummaryDto(
    Guid TenantId,
    int RequestCount,
    int CompletedCount,
    int FailedCount,
    int TotalTokens,
    decimal TotalCostUsd,
    double AverageLatencyMs);

public sealed record AIProviderResponse(string Output, int PromptTokens, int CompletionTokens, decimal CostUsd, string Model);
