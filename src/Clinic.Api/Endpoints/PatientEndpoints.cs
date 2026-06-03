using Clinic.Application.Common.Security;
using Clinic.Application.Patients.Commands;
using Clinic.Application.Patients.Queries;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients")
            .RequireAuthorization(AuthorizationPolicyNames.ManagePatients)
            .WithTags("Patients");

        group.MapGet("/", async (
            Guid? tenantId,
            Guid? locationId,
            string? search,
            int? pageNumber,
            int? pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new SearchPatientsQuery(
                tenantId,
                locationId,
                search,
                pageNumber ?? 1,
                pageSize ?? 20), cancellationToken)))
            .WithName("SearchPatients")
            .WithSummary("Search patients")
            .WithDescription("Returns a paged patient list filtered by tenant, location, or free-text search.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendQuery(new GetPatientByIdQuery(id), sender, cancellationToken))
            .WithName("GetPatient")
            .WithSummary("Get patient profile")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePatientCommand command, ISender sender, CancellationToken cancellationToken) =>
            await SendCreatedCommand(command, sender, patient => $"/api/patients/{patient.Id}", cancellationToken))
            .WithName("CreatePatient")
            .WithSummary("Create patient")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdatePatientRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCommand(new UpdatePatientCommand(
                id,
                request.LocationId,
                request.MedicalRecordNumber,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.BirthDate,
                request.Gender,
                request.Email,
                request.Phone,
                request.Address), sender, cancellationToken))
            .WithName("UpdatePatient")
            .WithSummary("Update patient")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendNoContentCommand(new DeletePatientCommand(id), sender, cancellationToken))
            .WithName("DeletePatient")
            .WithSummary("Soft delete patient")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/contacts", async (
            Guid id,
            CreatePatientContactRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCreatedCommand(new CreatePatientContactCommand(
                id,
                request.Name,
                request.Relationship,
                request.Email,
                request.Phone,
                request.IsPrimary), sender, contact => $"/api/patients/{id}/contacts/{contact.Id}", cancellationToken))
            .WithName("CreatePatientContact")
            .WithSummary("Create patient contact")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/contacts/{contactId:guid}", async (
            Guid id,
            Guid contactId,
            UpdatePatientContactRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCommand(new UpdatePatientContactCommand(
                id,
                contactId,
                request.Name,
                request.Relationship,
                request.Email,
                request.Phone,
                request.IsPrimary), sender, cancellationToken))
            .WithName("UpdatePatientContact")
            .WithSummary("Update patient contact")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/contacts/{contactId:guid}", async (
            Guid id,
            Guid contactId,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendNoContentCommand(new DeletePatientContactCommand(id, contactId), sender, cancellationToken))
            .WithName("DeletePatientContact")
            .WithSummary("Soft delete patient contact")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/documents", async (
            Guid id,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            await using var stream = file.OpenReadStream();
            return await SendCreatedCommand(new UploadPatientDocumentCommand(
                id,
                file.FileName,
                file.ContentType,
                file.Length,
                stream), sender, document => $"/api/patients/{id}/documents/{document.Id}", cancellationToken);
        })
        .DisableAntiforgery()
        .WithName("UploadPatientDocument")
        .WithSummary("Upload patient document")
        .WithDescription("Uploads a patient document to MinIO and stores document metadata.")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/timeline", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendQuery(new GetPatientTimelineQuery(id), sender, cancellationToken))
            .WithName("GetPatientTimeline")
            .WithSummary("Get patient timeline")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> SendQuery<TResponse>(
        IRequest<TResponse> query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await sender.Send(query, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
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
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
    }

    private static async Task<IResult> SendNoContentCommand(
        IRequest command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
    }

    private static IResult ToValidationProblem(ValidationException exception) =>
        Results.ValidationProblem(exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));

    private sealed record UpdatePatientRequest(
        Guid LocationId,
        string MedicalRecordNumber,
        string FirstName,
        string? MiddleName,
        string LastName,
        DateOnly BirthDate,
        string Gender,
        string Email,
        string Phone,
        string Address);

    private sealed record CreatePatientContactRequest(string Name, string Relationship, string Email, string Phone, bool IsPrimary);
    private sealed record UpdatePatientContactRequest(string Name, string Relationship, string Email, string Phone, bool IsPrimary);
}
