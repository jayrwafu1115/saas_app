namespace Clinic.Api.Options;

public sealed class IpRestrictionOptions
{
    public const string SectionName = "IpRestrictions";

    public bool Enabled { get; set; }
    public string[] Allowed { get; set; } = [];
    public string[] Blocked { get; set; } = [];
}
