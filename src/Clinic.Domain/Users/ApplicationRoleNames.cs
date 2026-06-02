namespace Clinic.Domain.Users;

public static class ApplicationRoleNames
{
    public const string SuperAdmin = "Super Admin";
    public const string ClinicOwner = "Clinic Owner";
    public const string ClinicAdmin = "Clinic Admin";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";
    public const string Patient = "Patient";

    public static readonly string[] All =
    [
        SuperAdmin,
        ClinicOwner,
        ClinicAdmin,
        Doctor,
        Nurse,
        Receptionist,
        Patient
    ];
}
