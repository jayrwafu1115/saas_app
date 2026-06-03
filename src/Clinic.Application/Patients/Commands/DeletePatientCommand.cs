using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed record DeletePatientCommand(Guid Id) : IRequest;
