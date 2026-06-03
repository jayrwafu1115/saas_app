namespace Clinic.Infrastructure.AI;

public sealed class AIProviderOptions
{
    public const string SectionName = "AI";

    public OpenAIOptions OpenAI { get; set; } = new();
    public OllamaOptions Ollama { get; set; } = new();
}

public sealed class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string Model { get; set; } = "gpt-4o-mini";
    public decimal PromptCostPer1KTokens { get; set; } = 0.00015m;
    public decimal CompletionCostPer1KTokens { get; set; } = 0.0006m;
}

public sealed class OllamaOptions
{
    public string Endpoint { get; set; } = "http://localhost:11434/api/generate";
    public string Model { get; set; } = "llama3.1";
}
