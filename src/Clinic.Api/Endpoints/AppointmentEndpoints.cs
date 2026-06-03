using Clinic.Application.Appointments.Commands;
using Clinic.Application.Appointments.Queries;
using Clinic.Application.Common.Security;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .RequireAuthorization(AuthorizationPolicyNames.ManageAppointments)
            .WithTags("Appointments");

        group.MapGet("/calendar", async (
            Guid? tenantId,
            Guid? locationId,
            Guid? doctorUserId,
            string? view,
            DateOnly? date,
            ISender sender,
            CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetAppointmentCalendarQuery(
                tenantId,
                locationId,
                doctorUserId,
                view ?? "daily",
                date ?? DateOnly.FromDateTime(DateTime.UtcNow)), cancellationToken)))
            .WithName("GetAppointmentCalendar")
            .WithSummary("Get appointment calendar")
            .WithDescription("Returns appointment events for daily, weekly, or monthly calendar views.")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateAppointmentCommand command, ISender sender, CancellationToken cancellationToken) =>
            await SendCreatedCommand(command, sender, appointment => $"/api/appointments/{appointment.Id}", cancellationToken))
            .WithName("CreateAppointment")
            .WithSummary("Create appointment")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/reschedule", async (
            Guid id,
            RescheduleAppointmentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCommand(new RescheduleAppointmentCommand(
                id,
                request.LocationId,
                request.DoctorUserId,
                request.StartsAtUtc,
                request.EndsAtUtc), sender, cancellationToken))
            .WithName("RescheduleAppointment")
            .WithSummary("Reschedule appointment")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendCommand(new CancelAppointmentCommand(id), sender, cancellationToken))
            .WithName("CancelAppointment")
            .WithSummary("Cancel appointment")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/check-in", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendCommand(new CheckInAppointmentCommand(id), sender, cancellationToken))
            .WithName("CheckInAppointment")
            .WithSummary("Check in appointment")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/check-out", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendCommand(new CheckOutAppointmentCommand(id), sender, cancellationToken))
            .WithName("CheckOutAppointment")
            .WithSummary("Check out appointment")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> SendCommand<TResponse>(
        IRequest<TResponse> command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await sender.Send(command, cancellationToken));
        }
        catch (ValidationException exception)
        {
            return ToValidationProblem(exception);
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
    }

    private static async Task<IResult> SendCreatedCommand<TResponse>(
        IRequest<TResponse> command,
        ISender sender,
        Func<TResponse, string> locationFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await sender.Send(command, cancellationToken);
            return Results.Created(locationFactory(response), response);
        }
        catch (ValidationException exception)
        {
            return ToValidationProblem(exception);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
    }

    private static IResult ToValidationProblem(ValidationException exception) =>
        Results.ValidationProblem(exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));

    private sealed record RescheduleAppointmentRequest(
        Guid LocationId,
        Guid DoctorUserId,
        DateTimeOffset StartsAtUtc,
        DateTimeOffset EndsAtUtc);
}
