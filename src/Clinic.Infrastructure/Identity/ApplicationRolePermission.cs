namespace Clinic.Infrastructure.Identity;

public sealed class ApplicationRolePermission
{
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
