using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed record UpdatePatientCommand(
    Guid Id,
    Guid LocationId,
    string MedicalRecordNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly BirthDate,
    string Gender,
    string Email,
    string Phone,
    string Address) : IRequest<PatientDto>;
