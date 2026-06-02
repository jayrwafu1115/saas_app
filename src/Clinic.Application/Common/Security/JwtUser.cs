namespace Clinic.Application.Common.Security;

public sealed record JwtUser(
    Guid Id,
    Guid? TenantId,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
