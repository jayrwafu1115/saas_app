namespace Clinic.Application.Common.Security;

public static class PermissionNames
{
    public const string ManageTenants = "tenants.manage";
    public const string ManageLocations = "locations.manage";
    public const string ManageRoles = "roles.manage";
    public const string AccessClinicalWorkspace = "clinical.access";
    public const string AccessPatientPortal = "patient.access";

    public static readonly string[] All =
    [
        ManageTenants,
        ManageLocations,
        ManageRoles,
        AccessClinicalWorkspace,
        AccessPatientPortal
    ];
}
