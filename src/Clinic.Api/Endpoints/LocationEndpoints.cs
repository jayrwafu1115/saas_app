using Clinic.Application.Locations.Commands;
using Clinic.Application.Locations.Queries;
using Clinic.Application.Common.Security;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/locations")
            .RequireAuthorization(AuthorizationPolicyNames.ManageLocations)
            .WithTags("Locations");

        group.MapGet("/", async (Guid? tenantId, ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetLocationsQuery(tenantId), cancellationToken)))
            .WithName("GetLocations")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", async (
            CreateLocationCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCommand(command, sender, cancellationToken))
            .WithName("CreateLocation")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> SendCommand(
        CreateLocationCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var location = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/locations/{location.Id}", location);
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
