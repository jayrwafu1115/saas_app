using MediatR;

namespace Clinic.Application.Patients.Queries;

public sealed record GetPatientByIdQuery(Guid Id) : IRequest<PatientDetailDto>;
