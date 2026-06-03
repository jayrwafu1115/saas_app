using Clinic.Domain.Patients;
using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class CreatePatientContactCommandHandler(IPatientRepository patients)
    : IRequestHandler<CreatePatientContactCommand, PatientContactDto>
{
    public async Task<PatientContactDto> Handle(CreatePatientContactCommand request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found.");
        }

        var contact = new PatientContact(
            request.PatientId,
            request.Name,
            request.Relationship,
            request.Email,
            request.Phone,
            request.IsPrimary);

        await patients.AddContactAsync(contact, cancellationToken);
        return contact.ToDto();
    }
}
