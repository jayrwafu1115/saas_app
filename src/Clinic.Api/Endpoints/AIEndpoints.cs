using Clinic.Application.AI.Commands;
using Clinic.Application.AI.Queries;
using Clinic.Application.Common.Security;
using Clinic.Domain.AI;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class AIEndpoints
{
    public static IEndpointRouteBuilder MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .RequireAuthorization(AuthorizationPolicyNames.ManageAI)
            .WithTags("AI");

        group.MapPost("/soap-note", async (AIGenerationRequest request, ISender sender, CancellationToken cancellationToken) =>
            await QueueAsync(request, AIGenerationType.SoapNote, sender, cancellationToken))
            .WithName("GenerateSoapNote")
            .WithSummary("Queue SOAP note generation")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/clinical-summary", async (AIGenerationRequest request, ISender sender, CancellationToken cancellationToken) =>
            await QueueAsync(request, AIGenerationType.ClinicalSummary, sender, cancellationToken))
            .WithName("GenerateClinicalSummary")
            .WithSummary("Queue clinical summary generation")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/prescription-summary", async (AIGenerationRequest request, ISender sender, CancellationToken cancellationToken) =>
            await QueueAsync(request, AIGenerationType.PrescriptionSummary, sender, cancellationToken))
            .WithName("GeneratePrescriptionSummary")
            .WithSummary("Queue prescription summary generation")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/visit-summary", async (AIGenerationRequest request, ISender sender, CancellationToken cancellationToken) =>
            await QueueAsync(request, AIGenerationType.VisitSummary, sender, cancellationToken))
            .WithName("GenerateVisitSummary")
            .WithSummary("Queue visit summary generation")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/generations/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await SendQuery(new GetAIGenerationQuery(id), sender, cancellationToken))
            .WithName("GetAIGeneration")
            .WithSummary("Get AI generation status and output")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/encounters/{encounterId:guid}/generations", async (Guid encounterId, ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new ListEncounterAIGenerationsQuery(encounterId), cancellationToken)))
            .WithName("ListEncounterAIGenerations")
            .WithSummary("List AI outputs for an encounter")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/usage", async (Guid tenantId, ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetAIUsageQuery(tenantId), cancellationToken)))
            .WithName("GetAIUsage")
            .WithSummary("Get AI usage, cost, and latency metrics")
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> QueueAsync(AIGenerationRequest request, AIGenerationType type, ISender sender, CancellationToken cancellationToken)
    {
        try
        {
            var generation = await sender.Send(new QueueAIGenerationCommand(request.EncounterId, type, request.Provider, request.Model), cancellationToken);
            return Results.Accepted($"/api/ai/generations/{generation.Id}", generation);
        }
        catch (ValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
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

    private sealed record AIGenerationRequest(Guid EncounterId, string Provider, string? Model);
}
