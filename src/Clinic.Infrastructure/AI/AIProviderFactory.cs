using Clinic.Application.AI;

namespace Clinic.Infrastructure.AI;

public sealed class AIProviderFactory(IEnumerable<IAIProvider> providers) : IAIProviderFactory
{
    public IAIProvider GetProvider(string providerName) =>
        providers.FirstOrDefault(provider => provider.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"AI provider '{providerName}' is not configured.");
}
