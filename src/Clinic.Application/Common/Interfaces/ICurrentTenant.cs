namespace Clinic.Application.Common.Interfaces;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
}
