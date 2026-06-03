using MediatR;

namespace Clinic.Application.Patients.Queries;

public sealed record GetPatientTimelineQuery(Guid PatientId) : IRequest<IReadOnlyList<PatientTimelineEventDto>>;
