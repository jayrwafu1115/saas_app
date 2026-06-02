using Clinic.Domain.Users;
using FluentAssertions;

namespace Clinic.Tests.Architecture;

public sealed class ApplicationRoleNamesTests
{
    [Fact]
    public void All_roles_are_registered_once()
    {
        ApplicationRoleNames.All.Should().HaveCount(7);
        ApplicationRoleNames.All.Should().OnlyHaveUniqueItems();
        ApplicationRoleNames.All.Should().Contain([
            ApplicationRoleNames.SuperAdmin,
            ApplicationRoleNames.ClinicOwner,
            ApplicationRoleNames.ClinicAdmin,
            ApplicationRoleNames.Doctor,
            ApplicationRoleNames.Nurse,
            ApplicationRoleNames.Receptionist,
            ApplicationRoleNames.Patient
        ]);
    }
}
