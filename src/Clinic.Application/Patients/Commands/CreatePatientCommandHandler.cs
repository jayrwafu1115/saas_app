using Clinic.Application.Locations;
using Clinic.Domain.Patients;
using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class CreatePatientCommandHandler(
    IPatientRepository patients,
    ILocationRepository locations)
    : IRequestHandler<CreatePatientCommand, PatientDto>
{
    public async Task<PatientDto> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var locationExists = (await locations.ListAsync(request.TenantId, cancellationToken))
            .Any(location => location.Id == request.LocationId);
        if (!locationExists)
        {
            throw new InvalidOperationException("Location does not exist for this tenant.");
        }

        if (await patients.MedicalRecordNumberExistsAsync(request.TenantId, request.MedicalRecordNumber, null, cancellationToken))
        {
            throw new InvalidOperationException("A patient with this medical record number already exists.");
        }

        var patient = new Patient(
            request.TenantId,
            request.LocationId,
            request.MedicalRecordNumber,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.BirthDate,
            request.Gender,
            request.Email,
            request.Phone,
            request.Address);

        await patients.AddAsync(patient, cancellationToken);
        return patient.ToDto();
    }
}
