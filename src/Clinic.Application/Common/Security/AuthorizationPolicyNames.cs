namespace Clinic.Application.Common.Security;

public static class AuthorizationPolicyNames
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string ManageTenants = "ManageTenants";
    public const string ManageLocations = "ManageLocations";
    public const string ManageRoles = "ManageRoles";
    public const string ManagePatients = "ManagePatients";
    public const string ManageAppointments = "ManageAppointments";
    public const string ManageEncounters = "ManageEncounters";
    public const string ManageAI = "ManageAI";
    public const string ViewReports = "ViewReports";
    public const string ManageBilling = "ManageBilling";
}
