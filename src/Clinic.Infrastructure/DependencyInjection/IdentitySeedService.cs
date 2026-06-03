using Clinic.Application.Common.Security;
using Clinic.Domain.Users;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Infrastructure.DependencyInjection;

public static class IdentitySeedService
{
    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        [ApplicationRoleNames.SuperAdmin] = PermissionNames.All,
        [ApplicationRoleNames.ClinicOwner] = [PermissionNames.ManageLocations, PermissionNames.ManagePatients, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.ClinicAdmin] = [PermissionNames.ManageLocations, PermissionNames.ManagePatients, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Doctor] = [PermissionNames.ManagePatients, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Nurse] = [PermissionNames.ManagePatients, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Receptionist] = [PermissionNames.ManagePatients, PermissionNames.ManageLocations, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Patient] = [PermissionNames.AccessPatientPortal]
    };

    private static readonly Dictionary<string, string> PermissionDescriptions = new()
    {
        [PermissionNames.ManageTenants] = "Manage tenant records.",
        [PermissionNames.ManageLocations] = "Manage clinic locations.",
        [PermissionNames.ManageRoles] = "Manage roles and user role assignments.",
        [PermissionNames.ManagePatients] = "Manage patient records and documents.",
        [PermissionNames.AccessClinicalWorkspace] = "Access staff clinical workspace.",
        [PermissionNames.AccessPatientPortal] = "Access patient portal."
    };

    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<Persistence.ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        foreach (var roleName in ApplicationRoleNames.All)
        {
            if (await roleManager.FindByNameAsync(roleName) is null)
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} application role."
                });
            }
        }

        foreach (var permissionName in PermissionNames.All)
        {
            if (!await dbContext.Permissions.AnyAsync(permission => permission.Name == permissionName))
            {
                dbContext.Permissions.Add(new Permission
                {
                    Name = permissionName,
                    Description = PermissionDescriptions[permissionName]
                });
            }
        }

        await dbContext.SaveChangesAsync();

        var roles = await dbContext.Roles.ToListAsync();
        var permissions = await dbContext.Permissions.ToListAsync();
        foreach (var (roleName, permissionNames) in RolePermissions)
        {
            var role = roles.Single(role => role.Name == roleName);
            foreach (var permissionName in permissionNames)
            {
                var permission = permissions.Single(permission => permission.Name == permissionName);
                var exists = await dbContext.RolePermissions.AnyAsync(rolePermission =>
                    rolePermission.RoleId == role.Id && rolePermission.PermissionId == permission.Id);
                if (!exists)
                {
                    dbContext.RolePermissions.Add(new ApplicationRolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();

        var seedSection = configuration.GetSection(IdentitySeedOptions.SectionName);
        var seedOptions = new IdentitySeedOptions
        {
            Email = seedSection["Email"] ?? string.Empty,
            Password = seedSection["Password"] ?? string.Empty,
            DisplayName = seedSection["DisplayName"] ?? "Super Admin"
        };
        if (seedOptions is null
            || string.IsNullOrWhiteSpace(seedOptions.Email)
            || string.IsNullOrWhiteSpace(seedOptions.Password))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(seedOptions.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = seedOptions.Email,
                Email = seedOptions.Email,
                DisplayName = seedOptions.DisplayName,
                EmailConfirmed = true
            };
            var created = await userManager.CreateAsync(user, seedOptions.Password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", created.Errors.Select(error => error.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoleNames.SuperAdmin))
        {
            await userManager.AddToRoleAsync(user, ApplicationRoleNames.SuperAdmin);
        }
    }
}
