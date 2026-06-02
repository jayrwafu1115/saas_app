using Clinic.Application.Tenants.Commands;
using Clinic.Application.Tenants.Queries;
using Clinic.Application.Common.Security;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .RequireAuthorization(AuthorizationPolicyNames.ManageTenants)
            .WithTags("Tenants");

        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetTenantsQuery(), cancellationToken)))
            .WithName("GetTenants")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", async (
            CreateTenantCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCommand(command, sender, cancellationToken))
            .WithName("CreateTenant")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> SendCommand(
        CreateTenantCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/tenants/{tenant.Id}", tenant);
        }
        catch (ValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
    }
}
