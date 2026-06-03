using MediatR;

namespace Clinic.Application.Clinical.Queries;

public sealed record GetPatientEncounterTimelineQuery(Guid PatientId) : IRequest<IReadOnlyList<EncounterTimelineEventDto>>;
