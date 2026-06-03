using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed record CreateEncounterCommand(
    Guid TenantId,
    Guid LocationId,
    Guid PatientId,
    Guid ClinicianUserId,
    Guid? AppointmentId,
    DateTimeOffset EncounterDateUtc,
    string ChiefComplaint,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string? Notes) : IRequest<EncounterDto>;
