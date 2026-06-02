using Clinic.Domain.Common;

namespace Clinic.Domain.Tenants;

public sealed class Tenant : BaseEntity
{
    private Tenant()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    public Tenant(string name, string slug)
    {
        Name = name;
        Slug = slug;
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public bool IsActive { get; private set; }
}
