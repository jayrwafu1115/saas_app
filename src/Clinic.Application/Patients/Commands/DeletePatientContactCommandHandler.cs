using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class DeletePatientContactCommandHandler(IPatientRepository patients)
    : IRequestHandler<DeletePatientContactCommand>
{
    public async Task Handle(DeletePatientContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await patients.GetContactByIdAsync(request.PatientId, request.ContactId, cancellationToken);
        if (contact is null)
        {
            throw new KeyNotFoundException("Patient contact was not found.");
        }

        contact.IsDeleted = true;
        await patients.SaveChangesAsync(cancellationToken);
    }
}
