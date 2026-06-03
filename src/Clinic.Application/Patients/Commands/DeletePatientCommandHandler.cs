using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class DeletePatientCommandHandler(IPatientRepository patients)
    : IRequestHandler<DeletePatientCommand>
{
    public async Task Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found.");
        }

        patient.IsDeleted = true;
        await patients.SaveChangesAsync(cancellationToken);
    }
}
