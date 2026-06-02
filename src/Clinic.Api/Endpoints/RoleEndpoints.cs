using Clinic.Application.Common.Security;
using Clinic.Infrastructure.Identity;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .RequireAuthorization(AuthorizationPolicyNames.ManageRoles)
            .WithTags("Roles");

        group.MapGet("/", async (ApplicationDbContext dbContext) =>
        {
            var roles = (await dbContext.Roles
                .AsNoTracking()
                .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
                .OrderBy(role => role.Name)
                .ToListAsync())
                .Select(role => new RoleResponse(
                    role.Id,
                    role.Name ?? string.Empty,
                    role.Description,
                    role.RolePermissions
                        .Select(rolePermission => rolePermission.Permission.Name)
                        .OrderBy(permission => permission)
                        .ToList()))
                .ToList();

            return Results.Ok(roles);
        })
        .WithName("GetRoles")
        .Produces<IReadOnlyList<RoleResponse>>();

        group.MapPost("/assign", async (
            AssignRoleRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                return Results.NotFound(new { message = "User was not found." });
            }

            if (request.Role == Domain.Users.ApplicationRoleNames.Patient)
            {
                return Results.BadRequest(new { message = "Patient role assignment is not available yet." });
            }

            if (!Domain.Users.ApplicationRoleNames.All.Contains(request.Role))
            {
                return Results.BadRequest(new { message = "Unknown role." });
            }

            if (!await userManager.IsInRoleAsync(user, request.Role))
            {
                var result = await userManager.AddToRoleAsync(user, request.Role);
                if (!result.Succeeded)
                {
                    return Results.ValidationProblem(result.Errors
                        .GroupBy(error => error.Code)
                        .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
                }
            }

            return Results.Ok(new { message = "Role assigned." });
        })
        .WithName("AssignRole")
        .Produces(StatusCodes.Status200OK);

        return app;
    }

    private sealed record AssignRoleRequest(Guid UserId, string Role);
    private sealed record RoleResponse(Guid Id, string Name, string Description, IEnumerable<string> Permissions);
}
