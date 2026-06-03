using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Clinical;
using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed class UpdateEncounterSoapCommandHandler(IEncounterRepository encounters, ICurrentUser currentUser)
    : IRequestHandler<UpdateEncounterSoapCommand, EncounterDto>
{
    public async Task<EncounterDto> Handle(UpdateEncounterSoapCommand request, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(request.EncounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        encounter.UpdateSoap(
            request.LocationId,
            request.ClinicianUserId,
            request.EncounterDateUtc,
            request.ChiefComplaint,
            request.Subjective,
            request.Objective,
            request.Assessment,
            request.Plan,
            request.Notes);
        await encounters.AddAuditLogAsync(new EncounterAuditLog(encounter.TenantId, encounter.Id, "encounter.updated", "SOAP note updated.", currentUser.UserId), cancellationToken);
        await encounters.SaveChangesAsync(cancellationToken);
        return encounter.ToDto();
    }
}
