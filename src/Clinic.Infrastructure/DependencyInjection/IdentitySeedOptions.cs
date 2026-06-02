namespace Clinic.Infrastructure.DependencyInjection;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "SeedData:SuperAdmin";

    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DisplayName { get; init; } = "Super Admin";
}
