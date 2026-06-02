using MediatR;

namespace Clinic.Application.Tenants.Commands;

public sealed record CreateTenantCommand(string Name, string Slug, string Status, string SettingsJson)
    : IRequest<TenantDto>;
