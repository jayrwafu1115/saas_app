namespace Clinic.Application.Common.Security;

public static class AuthorizationPolicyNames
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string ManageTenants = "ManageTenants";
    public const string ManageLocations = "ManageLocations";
    public const string ManageRoles = "ManageRoles";
}
