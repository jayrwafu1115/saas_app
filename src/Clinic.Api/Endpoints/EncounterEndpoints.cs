using System.Net;
using System.Text;
using Clinic.Application.Clinical;
using Clinic.Application.Clinical.Commands;
using Clinic.Application.Clinical.Queries;
using Clinic.Application.Common.Security;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class EncounterEndpoints
{
    public static IEndpointRouteBuilder MapEncounterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/encounters")
            .RequireAuthorization(AuthorizationPolicyNames.ManageEncounters)
            .WithTags("Encounters");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendQuery(new GetEncounterByIdQuery(id), sender, cancellationToken))
            .WithName("GetEncounter")
            .WithSummary("Get clinical encounter")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateEncounterCommand command, ISender sender, CancellationToken cancellationToken) =>
            await SendCreatedCommand(command, sender, encounter => $"/api/encounters/{encounter.Id}", cancellationToken))
            .WithName("CreateEncounter")
            .WithSummary("Create SOAP encounter")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}/soap", async (
            Guid id,
            UpdateEncounterSoapRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCommand(new UpdateEncounterSoapCommand(
                id,
                request.LocationId,
                request.ClinicianUserId,
                request.EncounterDateUtc,
                request.ChiefComplaint,
                request.Subjective,
                request.Objective,
                request.Assessment,
                request.Plan,
                request.Notes), sender, cancellationToken))
            .WithName("UpdateEncounterSoap")
            .WithSummary("Update SOAP notes")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/vitals", async (
            Guid id,
            AddVitalRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCreatedCommand(new AddVitalCommand(
                id,
                request.RecordedAtUtc,
                request.TemperatureCelsius,
                request.SystolicBloodPressure,
                request.DiastolicBloodPressure,
                request.HeartRate,
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.HeightCm,
                request.WeightKg,
                request.Notes), sender, vital => $"/api/encounters/{id}/vitals/{vital.Id}", cancellationToken))
            .WithName("AddEncounterVital")
            .WithSummary("Record vitals")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/diagnoses", async (
            Guid id,
            AddDiagnosisRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCreatedCommand(new AddDiagnosisCommand(id, request.Code, request.Description, request.Type), sender, diagnosis => $"/api/encounters/{id}/diagnoses/{diagnosis.Id}", cancellationToken))
            .WithName("AddEncounterDiagnosis")
            .WithSummary("Add diagnosis")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/prescriptions", async (
            Guid id,
            AddPrescriptionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
            await SendCreatedCommand(new AddPrescriptionCommand(
                id,
                request.MedicationName,
                request.Dosage,
                request.Frequency,
                request.Duration,
                request.Instructions), sender, prescription => $"/api/encounters/{id}/prescriptions/{prescription.Id}", cancellationToken))
            .WithName("AddEncounterPrescription")
            .WithSummary("Add prescription")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/sign", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendCommand(new SignEncounterCommand(id), sender, cancellationToken))
            .WithName("SignEncounter")
            .WithSummary("Sign encounter")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/patients/{patientId:guid}/timeline", async (Guid patientId, ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetPatientEncounterTimelineQuery(patientId), cancellationToken)))
            .WithName("GetEncounterTimeline")
            .WithSummary("Get clinical encounter timeline")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}/print", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var encounter = await sender.Send(new GetEncounterByIdQuery(id), cancellationToken);
            return Results.Content(BuildPrintHtml(encounter), "text/html; charset=utf-8");
        })
        .WithName("PrintEncounter")
        .WithSummary("Render printable encounter view")
        .Produces(StatusCodes.Status200OK, contentType: "text/html");

        group.MapGet("/{id:guid}/pdf", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var encounter = await sender.Send(new GetEncounterByIdQuery(id), cancellationToken);
            return Results.File(BuildPdf(encounter), "application/pdf", $"encounter-{encounter.Id}.pdf");
        })
        .WithName("ExportEncounterPdf")
        .WithSummary("Export encounter PDF")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf");

        return app;
    }

    private static string BuildPrintHtml(EncounterDetailDto encounter) =>
        $$"""
        <!doctype html>
        <html>
        <head>
          <meta charset="utf-8">
          <title>Encounter {{encounter.Id}}</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 32px; color: #172033; }
            h1, h2 { margin-bottom: 8px; }
            section { border-top: 1px solid #d8dde8; padding-top: 16px; margin-top: 18px; }
            .meta { color: #526071; font-size: 13px; }
            table { width: 100%; border-collapse: collapse; margin-top: 8px; }
            th, td { border: 1px solid #d8dde8; padding: 8px; text-align: left; }
          </style>
        </head>
        <body>
          <h1>Clinical Encounter</h1>
          <p class="meta">Date: {{WebUtility.HtmlEncode(encounter.EncounterDateUtc.ToString("u"))}} | Status: {{encounter.Status}}</p>
          <section><h2>SOAP</h2><p><strong>Chief complaint:</strong> {{WebUtility.HtmlEncode(encounter.ChiefComplaint)}}</p><p><strong>Subjective:</strong> {{WebUtility.HtmlEncode(encounter.Subjective)}}</p><p><strong>Objective:</strong> {{WebUtility.HtmlEncode(encounter.Objective)}}</p><p><strong>Assessment:</strong> {{WebUtility.HtmlEncode(encounter.Assessment)}}</p><p><strong>Plan:</strong> {{WebUtility.HtmlEncode(encounter.Plan)}}</p></section>
          <section><h2>Diagnoses</h2>{{BuildDiagnosisTable(encounter)}}</section>
          <section><h2>Prescriptions</h2>{{BuildPrescriptionTable(encounter)}}</section>
        </body>
        </html>
        """;

    private static string BuildDiagnosisTable(EncounterDetailDto encounter)
    {
        if (encounter.Diagnoses.Count == 0)
        {
            return "<p>No diagnoses recorded.</p>";
        }

        return "<table><thead><tr><th>Code</th><th>Description</th><th>Type</th></tr></thead><tbody>"
            + string.Join("", encounter.Diagnoses.Select(diagnosis => $"<tr><td>{WebUtility.HtmlEncode(diagnosis.Code)}</td><td>{WebUtility.HtmlEncode(diagnosis.Description)}</td><td>{WebUtility.HtmlEncode(diagnosis.Type)}</td></tr>"))
            + "</tbody></table>";
    }

    private static string BuildPrescriptionTable(EncounterDetailDto encounter)
    {
        if (encounter.Prescriptions.Count == 0)
        {
            return "<p>No prescriptions recorded.</p>";
        }

        return "<table><thead><tr><th>Medication</th><th>Dosage</th><th>Frequency</th><th>Duration</th></tr></thead><tbody>"
            + string.Join("", encounter.Prescriptions.Select(prescription => $"<tr><td>{WebUtility.HtmlEncode(prescription.MedicationName)}</td><td>{WebUtility.HtmlEncode(prescription.Dosage)}</td><td>{WebUtility.HtmlEncode(prescription.Frequency)}</td><td>{WebUtility.HtmlEncode(prescription.Duration)}</td></tr>"))
            + "</tbody></table>";
    }

    private static byte[] BuildPdf(EncounterDetailDto encounter)
    {
        var text = $"Clinical Encounter\nDate: {encounter.EncounterDateUtc:u}\nChief Complaint: {encounter.ChiefComplaint}\nSubjective: {encounter.Subjective}\nObjective: {encounter.Objective}\nAssessment: {encounter.Assessment}\nPlan: {encounter.Plan}";
        var stream = new MemoryStream();
        var body = $"BT /F1 12 Tf 72 740 Td ({EscapePdfText(text)}) Tj ET";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(body)} >>\nstream\n{body}\nendstream"
        };

        void Write(string value) => stream.Write(Encoding.ASCII.GetBytes(value));
        Write("%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(stream.Position);
            Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }

        var xref = stream.Position;
        Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            Write($"{offset:0000000000} 00000 n \n");
        }

        Write($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return stream.ToArray();
    }

    private static string EscapePdfText(string text) =>
        text.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    private static async Task<IResult> SendQuery<TResponse>(IRequest<TResponse> query, ISender sender, CancellationToken cancellationToken)
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

    private static async Task<IResult> SendCommand<TResponse>(IRequest<TResponse> command, ISender sender, CancellationToken cancellationToken)
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

    private static IResult ToValidationProblem(ValidationException exception) =>
        Results.ValidationProblem(exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));

    private sealed record UpdateEncounterSoapRequest(
        Guid LocationId,
        Guid ClinicianUserId,
        DateTimeOffset EncounterDateUtc,
        string ChiefComplaint,
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string? Notes);

    private sealed record AddVitalRequest(
        DateTimeOffset RecordedAtUtc,
        decimal? TemperatureCelsius,
        int? SystolicBloodPressure,
        int? DiastolicBloodPressure,
        int? HeartRate,
        int? RespiratoryRate,
        int? OxygenSaturation,
        decimal? HeightCm,
        decimal? WeightKg,
        string? Notes);

    private sealed record AddDiagnosisRequest(string Code, string Description, string Type);
    private sealed record AddPrescriptionRequest(string MedicationName, string Dosage, string Frequency, string Duration, string? Instructions);
}
