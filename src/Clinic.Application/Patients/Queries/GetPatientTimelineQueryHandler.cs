using MediatR;

namespace Clinic.Application.Patients.Queries;

public sealed class GetPatientTimelineQueryHandler(IPatientRepository patients)
    : IRequestHandler<GetPatientTimelineQuery, IReadOnlyList<PatientTimelineEventDto>>
{
    public async Task<IReadOnlyList<PatientTimelineEventDto>> Handle(GetPatientTimelineQuery request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found.");
        }

        var events = new List<PatientTimelineEventDto>
        {
            new(
                patient.CreatedAtUtc,
                "patient.created",
                "Patient created",
                $"{patient.FirstName} {patient.LastName} was registered with MRN {patient.MedicalRecordNumber}.")
        };

        events.AddRange(patient.Documents
            .Where(document => !document.IsDeleted)
            .Select(document => new PatientTimelineEventDto(
                document.UploadedAtUtc,
                "document.uploaded",
                "Document uploaded",
                document.FileName)));

        events.AddRange(patient.Contacts
            .Where(contact => !contact.IsDeleted)
            .Select(contact => new PatientTimelineEventDto(
                contact.CreatedAtUtc,
                "contact.added",
                "Contact added",
                $"{contact.Name} ({contact.Relationship})")));

        return events
            .OrderByDescending(item => item.OccurredAtUtc)
            .ToList();
    }
}
