using Clinic.Domain.Common;

namespace Clinic.Domain.Tenants;

public sealed class Location : BaseEntity
{
    private Location()
    {
        Name = string.Empty;
        Code = string.Empty;
        Address = string.Empty;
        Phone = string.Empty;
        Tenant = null!;
    }

    public Location(Guid tenantId, string name, string code, string address, string phone)
    {
        TenantId = tenantId;
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Address = address.Trim();
        Phone = phone.Trim();
        Tenant = null!;
    }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Address { get; private set; }
    public string Phone { get; private set; }
    public Tenant Tenant { get; private set; }
}
