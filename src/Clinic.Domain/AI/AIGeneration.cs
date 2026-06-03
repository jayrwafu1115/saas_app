using Clinic.Domain.Common;

namespace Clinic.Domain.AI;

public sealed class AIGeneration : BaseEntity
{
    private AIGeneration()
    {
        Provider = string.Empty;
        PromptHash = string.Empty;
        Prompt = string.Empty;
        Output = string.Empty;
        ErrorMessage = string.Empty;
        Model = string.Empty;
        RequestedBy = string.Empty;
    }

    public AIGeneration(Guid tenantId, Guid encounterId, AIGenerationType type, string provider, string model, string promptHash, string prompt, string? requestedBy)
    {
        TenantId = tenantId;
        EncounterId = encounterId;
        Type = type;
        Provider = provider.Trim().ToLowerInvariant();
        Model = model.Trim();
        PromptHash = promptHash;
        Prompt = prompt;
        RequestedBy = requestedBy ?? string.Empty;
        Status = AIGenerationStatus.Queued;
        Output = string.Empty;
        ErrorMessage = string.Empty;
    }

    public Guid TenantId { get; private set; }
    public Guid EncounterId { get; private set; }
    public AIGenerationType Type { get; private set; }
    public string Provider { get; private set; }
    public string Model { get; private set; }
    public string PromptHash { get; private set; }
    public string Prompt { get; private set; }
    public string Output { get; private set; }
    public AIGenerationStatus Status { get; private set; }
    public string ErrorMessage { get; private set; }
    public string RequestedBy { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public int TotalTokens { get; private set; }
    public decimal CostUsd { get; private set; }
    public long LatencyMs { get; private set; }
    public bool ServedFromCache { get; private set; }

    public void MarkProcessing(DateTimeOffset startedAtUtc)
    {
        Status = AIGenerationStatus.Processing;
        StartedAtUtc = startedAtUtc;
    }

    public void Complete(string output, int promptTokens, int completionTokens, decimal costUsd, long latencyMs, DateTimeOffset completedAtUtc, bool servedFromCache)
    {
        Output = output.Trim();
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
        TotalTokens = promptTokens + completionTokens;
        CostUsd = costUsd;
        LatencyMs = latencyMs;
        CompletedAtUtc = completedAtUtc;
        ServedFromCache = servedFromCache;
        Status = AIGenerationStatus.Completed;
        ErrorMessage = string.Empty;
    }

    public void Fail(string errorMessage, DateTimeOffset completedAtUtc)
    {
        ErrorMessage = errorMessage.Trim();
        CompletedAtUtc = completedAtUtc;
        Status = AIGenerationStatus.Failed;
    }
}
