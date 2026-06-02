namespace Clinic.Application.Locations;

public sealed record LocationDto(Guid Id, Guid TenantId, string Name, string Code, string Address, string Phone);
