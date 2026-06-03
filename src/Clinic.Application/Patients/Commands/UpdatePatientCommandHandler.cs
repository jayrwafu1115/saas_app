using Clinic.Application.Locations;
using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class UpdatePatientCommandHandler(
    IPatientRepository patients,
    ILocationRepository locations)
    : IRequestHandler<UpdatePatientCommand, PatientDto>
{
    public async Task<PatientDto> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found.");
        }

        var locationExists = (await locations.ListAsync(patient.TenantId, cancellationToken))
            .Any(location => location.Id == request.LocationId);
        if (!locationExists)
        {
            throw new InvalidOperationException("Location does not exist for this tenant.");
        }

        if (await patients.MedicalRecordNumberExistsAsync(patient.TenantId, request.MedicalRecordNumber, patient.Id, cancellationToken))
        {
            throw new InvalidOperationException("A patient with this medical record number already exists.");
        }

        patient.Update(
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

        await patients.SaveChangesAsync(cancellationToken);
        return patient.ToDto();
    }
}
