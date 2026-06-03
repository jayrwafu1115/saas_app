using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Clinical;
using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed class AddDiagnosisCommandHandler(IEncounterRepository encounters, ICurrentUser currentUser)
    : IRequestHandler<AddDiagnosisCommand, DiagnosisDto>
{
    public async Task<DiagnosisDto> Handle(AddDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(request.EncounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        var diagnosis = new Diagnosis(encounter.TenantId, encounter.Id, request.Code, request.Description, request.Type);
        await encounters.AddDiagnosisAsync(diagnosis, cancellationToken);
        await encounters.AddAuditLogAsync(new EncounterAuditLog(encounter.TenantId, encounter.Id, "diagnosis.added", $"{diagnosis.Code} added.", currentUser.UserId), cancellationToken);
        return diagnosis.ToDto();
    }
}
