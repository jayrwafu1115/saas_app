using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Clinical;
using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed class AddPrescriptionCommandHandler(IEncounterRepository encounters, ICurrentUser currentUser)
    : IRequestHandler<AddPrescriptionCommand, PrescriptionDto>
{
    public async Task<PrescriptionDto> Handle(AddPrescriptionCommand request, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(request.EncounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        var prescription = new Prescription(
            encounter.TenantId,
            encounter.Id,
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.Duration,
            request.Instructions);

        await encounters.AddPrescriptionAsync(prescription, cancellationToken);
        await encounters.AddAuditLogAsync(new EncounterAuditLog(encounter.TenantId, encounter.Id, "prescription.added", $"{prescription.MedicationName} prescribed.", currentUser.UserId), cancellationToken);
        return prescription.ToDto();
    }
}
