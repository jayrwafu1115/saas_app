using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Clinical;
using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed class AddVitalCommandHandler(IEncounterRepository encounters, ICurrentUser currentUser)
    : IRequestHandler<AddVitalCommand, VitalDto>
{
    public async Task<VitalDto> Handle(AddVitalCommand request, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(request.EncounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        var vital = new Vital(
            encounter.TenantId,
            encounter.Id,
            request.RecordedAtUtc,
            request.TemperatureCelsius,
            request.SystolicBloodPressure,
            request.DiastolicBloodPressure,
            request.HeartRate,
            request.RespiratoryRate,
            request.OxygenSaturation,
            request.HeightCm,
            request.WeightKg,
            request.Notes);

        await encounters.AddVitalAsync(vital, cancellationToken);
        await encounters.AddAuditLogAsync(new EncounterAuditLog(encounter.TenantId, encounter.Id, "vital.added", "Vitals recorded.", currentUser.UserId), cancellationToken);
        return vital.ToDto();
    }
}
