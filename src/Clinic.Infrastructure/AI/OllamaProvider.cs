using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Clinic.Application.AI;
using Clinic.Domain.AI;
using Microsoft.Extensions.Options;

namespace Clinic.Infrastructure.AI;

public sealed class OllamaProvider(HttpClient httpClient, IOptions<AIProviderOptions> options) : IAIProvider
{
    public string Name => "ollama";

    public async Task<AIProviderResponse> GenerateAsync(AIGenerationType type, string prompt, string? model, CancellationToken cancellationToken)
    {
        var ollama = options.Value.Ollama;
        using var response = await httpClient.PostAsJsonAsync(ollama.Endpoint, new OllamaRequest(
            string.IsNullOrWhiteSpace(model) ? ollama.Model : model,
            prompt,
            false), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var fallback = $"{type}: {prompt.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()}";
            return new AIProviderResponse(fallback, EstimateTokens(prompt), EstimateTokens(fallback), 0, model ?? ollama.Model);
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken);
        var output = payload?.Response ?? string.Empty;
        return new AIProviderResponse(output, EstimateTokens(prompt), EstimateTokens(output), 0, payload?.Model ?? ollama.Model);
    }

    private static int EstimateTokens(string value) =>
        Math.Max(1, value.Length / 4);

    private sealed record OllamaRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaResponse(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("response")] string Response);
}
