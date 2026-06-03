using MediatR;

namespace Clinic.Application.Clinical.Queries;

public sealed class GetPatientEncounterTimelineQueryHandler(IEncounterRepository encounters)
    : IRequestHandler<GetPatientEncounterTimelineQuery, IReadOnlyList<EncounterTimelineEventDto>>
{
    public async Task<IReadOnlyList<EncounterTimelineEventDto>> Handle(GetPatientEncounterTimelineQuery request, CancellationToken cancellationToken)
    {
        var patientEncounters = await encounters.ListByPatientAsync(request.PatientId, cancellationToken);
        return patientEncounters
            .Select(encounter => new EncounterTimelineEventDto(
                encounter.EncounterDateUtc,
                "encounter",
                encounter.ChiefComplaint,
                $"{encounter.Status} encounter with {encounter.Diagnoses.Count(diagnosis => !diagnosis.IsDeleted)} diagnoses and {encounter.Prescriptions.Count(prescription => !prescription.IsDeleted)} prescriptions."))
            .OrderByDescending(item => item.OccurredAtUtc)
            .ToList();
    }
}
