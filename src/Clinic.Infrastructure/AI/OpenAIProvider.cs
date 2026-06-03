using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Clinic.Application.AI;
using Clinic.Domain.AI;
using Microsoft.Extensions.Options;

namespace Clinic.Infrastructure.AI;

public sealed class OpenAIProvider(HttpClient httpClient, IOptions<AIProviderOptions> options) : IAIProvider
{
    public string Name => "openai";

    public async Task<AIProviderResponse> GenerateAsync(AIGenerationType type, string prompt, string? model, CancellationToken cancellationToken)
    {
        var openAi = options.Value.OpenAI;
        if (string.IsNullOrWhiteSpace(openAi.ApiKey))
        {
            return DeterministicFallback(type, prompt, model ?? openAi.Model);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, openAi.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAi.ApiKey);
        request.Content = JsonContent.Create(new OpenAIChatRequest(
            Model: string.IsNullOrWhiteSpace(model) ? openAi.Model : model,
            Messages:
            [
                new("system", "You are a clinical documentation assistant. Be concise, factual, and avoid adding unsupported facts."),
                new("user", prompt)
            ],
            Temperature: 0.2m));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>(cancellationToken);
        var output = payload?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
        var promptTokens = payload?.Usage?.PromptTokens ?? EstimateTokens(prompt);
        var completionTokens = payload?.Usage?.CompletionTokens ?? EstimateTokens(output);
        var cost = (promptTokens / 1000m * openAi.PromptCostPer1KTokens) + (completionTokens / 1000m * openAi.CompletionCostPer1KTokens);
        return new AIProviderResponse(output, promptTokens, completionTokens, cost, payload?.Model ?? openAi.Model);
    }

    private static AIProviderResponse DeterministicFallback(AIGenerationType type, string prompt, string model)
    {
        var output = $"{type}: {prompt.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()}";
        return new AIProviderResponse(output, EstimateTokens(prompt), EstimateTokens(output), 0, model);
    }

    private static int EstimateTokens(string value) =>
        Math.Max(1, value.Length / 4);

    private sealed record OpenAIChatRequest(string Model, IReadOnlyList<OpenAIMessage> Messages, decimal Temperature)
    {
        [JsonPropertyName("model")]
        public string Model { get; } = Model;

        [JsonPropertyName("messages")]
        public IReadOnlyList<OpenAIMessage> Messages { get; } = Messages;

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; } = Temperature;
    }

    private sealed record OpenAIMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OpenAIChatResponse(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("choices")] IReadOnlyList<OpenAIChoice> Choices,
        [property: JsonPropertyName("usage")] OpenAIUsage? Usage);

    private sealed record OpenAIChoice([property: JsonPropertyName("message")] OpenAIMessage Message);

    private sealed record OpenAIUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);
}
