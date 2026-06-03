using Clinic.Application.Common.Interfaces;
using Clinic.Application.Locations;
using Clinic.Application.Patients;
using Clinic.Domain.Clinical;
using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed class CreateEncounterCommandHandler(
    IEncounterRepository encounters,
    ILocationRepository locations,
    IPatientRepository patients,
    ICurrentUser currentUser)
    : IRequestHandler<CreateEncounterCommand, EncounterDto>
{
    public async Task<EncounterDto> Handle(CreateEncounterCommand request, CancellationToken cancellationToken)
    {
        var locationExists = (await locations.ListAsync(request.TenantId, cancellationToken))
            .Any(location => location.Id == request.LocationId);
        if (!locationExists)
        {
            throw new InvalidOperationException("Location does not exist for this tenant.");
        }

        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null || patient.TenantId != request.TenantId)
        {
            throw new InvalidOperationException("Patient does not exist for this tenant.");
        }

        var encounter = new Encounter(
            request.TenantId,
            request.LocationId,
            request.PatientId,
            request.ClinicianUserId,
            request.AppointmentId,
            request.EncounterDateUtc,
            request.ChiefComplaint,
            request.Subjective,
            request.Objective,
            request.Assessment,
            request.Plan,
            request.Notes);

        encounter.AuditLogs.Add(new EncounterAuditLog(request.TenantId, encounter.Id, "encounter.created", "Encounter draft created.", currentUser.UserId));

        await encounters.AddAsync(encounter, cancellationToken);
        return encounter.ToDto();
    }
}
