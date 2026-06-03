using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed record DeletePatientContactCommand(Guid PatientId, Guid ContactId) : IRequest;
