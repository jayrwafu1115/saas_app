namespace Clinic.Infrastructure.Identity;

public sealed class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<ApplicationRolePermission> RolePermissions { get; } = [];
}
