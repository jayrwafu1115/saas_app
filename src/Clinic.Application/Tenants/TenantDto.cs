namespace Clinic.Application.Tenants;

public sealed record TenantDto(Guid Id, string Name, string Slug, string Status, string SettingsJson);
