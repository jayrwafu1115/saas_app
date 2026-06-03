using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed record CreatePatientContactCommand(
    Guid PatientId,
    string Name,
    string Relationship,
    string Email,
    string Phone,
    bool IsPrimary) : IRequest<PatientContactDto>;
