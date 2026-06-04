using Clinic.Application.Common.Security;
using Clinic.Domain.Billing;
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
        [ApplicationRoleNames.ClinicOwner] = [PermissionNames.ManageLocations, PermissionNames.ManagePatients, PermissionNames.ManageAppointments, PermissionNames.ManageEncounters, PermissionNames.ManageAI, PermissionNames.ViewReports, PermissionNames.ManageBilling, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.ClinicAdmin] = [PermissionNames.ManageLocations, PermissionNames.ManagePatients, PermissionNames.ManageAppointments, PermissionNames.ManageEncounters, PermissionNames.ManageAI, PermissionNames.ViewReports, PermissionNames.ManageBilling, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Doctor] = [PermissionNames.ManagePatients, PermissionNames.ManageAppointments, PermissionNames.ManageEncounters, PermissionNames.ManageAI, PermissionNames.ViewReports, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Nurse] = [PermissionNames.ManagePatients, PermissionNames.ManageAppointments, PermissionNames.ManageEncounters, PermissionNames.ManageAI, PermissionNames.ViewReports, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Receptionist] = [PermissionNames.ManagePatients, PermissionNames.ManageAppointments, PermissionNames.ManageLocations, PermissionNames.AccessClinicalWorkspace],
        [ApplicationRoleNames.Patient] = [PermissionNames.AccessPatientPortal]
    };

    private static readonly Dictionary<string, string> PermissionDescriptions = new()
    {
        [PermissionNames.ManageTenants] = "Manage tenant records.",
        [PermissionNames.ManageLocations] = "Manage clinic locations.",
        [PermissionNames.ManageRoles] = "Manage roles and user role assignments.",
        [PermissionNames.ManagePatients] = "Manage patient records and documents.",
        [PermissionNames.ManageAppointments] = "Manage appointment scheduling and attendance.",
        [PermissionNames.ManageEncounters] = "Manage clinical encounters, vitals, diagnoses, and prescriptions.",
        [PermissionNames.ManageAI] = "Generate, inspect, and track clinical AI outputs.",
        [PermissionNames.ViewReports] = "View dashboards, analytics, and reporting exports.",
        [PermissionNames.ManageBilling] = "Manage subscriptions, billing providers, usage, and tenant restrictions.",
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
        await SeedPlansAsync(dbContext);

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

    private static async Task SeedPlansAsync(Persistence.ApplicationDbContext dbContext)
    {
        var plans = new[]
        {
            new SubscriptionPlan("Starter", "starter", 1499m, 5, 1, 1, 500, 14, "{\"features\":[\"patient-management\",\"appointments\",\"basic-reports\"]}"),
            new SubscriptionPlan("Professional", "professional", 4999m, 25, 8, 3, 5000, 14, "{\"features\":[\"patient-management\",\"appointments\",\"encounters\",\"ai\",\"reports\"]}"),
            new SubscriptionPlan("Enterprise", "enterprise", 14999m, 250, 100, 25, 100000, 30, "{\"features\":[\"all-modules\",\"priority-support\",\"advanced-analytics\"]}")
        };

        foreach (var plan in plans)
        {
            if (!await dbContext.SubscriptionPlans.AnyAsync(existing => existing.Code == plan.Code))
            {
                dbContext.SubscriptionPlans.Add(plan);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
