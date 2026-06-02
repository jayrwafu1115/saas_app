using Clinic.Domain.Common;

namespace Clinic.Domain.Tenants;

public sealed class Tenant : BaseEntity
{
    private Tenant()
    {
        Name = string.Empty;
        Slug = string.Empty;
        Status = string.Empty;
        SettingsJson = string.Empty;
    }

    public Tenant(string name, string slug, string status, string settingsJson)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Status = status.Trim();
        SettingsJson = settingsJson;
    }

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Status { get; private set; }
    public string SettingsJson { get; private set; }
}
