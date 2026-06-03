using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Clinical;
using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed class SignEncounterCommandHandler(
    IEncounterRepository encounters,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser)
    : IRequestHandler<SignEncounterCommand, EncounterDto>
{
    public async Task<EncounterDto> Handle(SignEncounterCommand request, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(request.EncounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        encounter.Sign(dateTimeProvider.UtcNow);
        await encounters.AddAuditLogAsync(new EncounterAuditLog(encounter.TenantId, encounter.Id, "encounter.signed", "Encounter signed.", currentUser.UserId), cancellationToken);
        await encounters.SaveChangesAsync(cancellationToken);
        return encounter.ToDto();
    }
}
