using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed record UpdateEncounterSoapCommand(
    Guid EncounterId,
    Guid LocationId,
    Guid ClinicianUserId,
    DateTimeOffset EncounterDateUtc,
    string ChiefComplaint,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string? Notes) : IRequest<EncounterDto>;
