using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class UpdatePatientContactCommandHandler(IPatientRepository patients)
    : IRequestHandler<UpdatePatientContactCommand, PatientContactDto>
{
    public async Task<PatientContactDto> Handle(UpdatePatientContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await patients.GetContactByIdAsync(request.PatientId, request.ContactId, cancellationToken);
        if (contact is null)
        {
            throw new KeyNotFoundException("Patient contact was not found.");
        }

        contact.Update(request.Name, request.Relationship, request.Email, request.Phone, request.IsPrimary);
        await patients.SaveChangesAsync(cancellationToken);
        return contact.ToDto();
    }
}
