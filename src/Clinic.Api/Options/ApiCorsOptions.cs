namespace Clinic.Api.Options;

public sealed class ApiCorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "ConfiguredCors";

    public string[] AllowedOrigins { get; init; } = [];
}
