namespace Clinic.Application.AI;

public interface IAIProviderFactory
{
    IAIProvider GetProvider(string providerName);
}
