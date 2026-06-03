using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed record UpdatePatientContactCommand(
    Guid PatientId,
    Guid ContactId,
    string Name,
    string Relationship,
    string Email,
    string Phone,
    bool IsPrimary) : IRequest<PatientContactDto>;
