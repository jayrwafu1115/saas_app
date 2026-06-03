using Clinic.Domain.Common;

namespace Clinic.Domain.Clinical;

public sealed class Encounter : BaseEntity
{
    private Encounter()
    {
        ChiefComplaint = string.Empty;
        Subjective = string.Empty;
        Objective = string.Empty;
        Assessment = string.Empty;
        Plan = string.Empty;
        Notes = string.Empty;
    }

    public Encounter(
        Guid tenantId,
        Guid locationId,
        Guid patientId,
        Guid clinicianUserId,
        Guid? appointmentId,
        DateTimeOffset encounterDateUtc,
        string chiefComplaint,
        string subjective,
        string objective,
        string assessment,
        string plan,
        string? notes)
    {
        TenantId = tenantId;
        LocationId = locationId;
        PatientId = patientId;
        ClinicianUserId = clinicianUserId;
        AppointmentId = appointmentId;
        EncounterDateUtc = encounterDateUtc;
        ChiefComplaint = chiefComplaint.Trim();
        Subjective = subjective.Trim();
        Objective = objective.Trim();
        Assessment = assessment.Trim();
        Plan = plan.Trim();
        Notes = notes?.Trim() ?? string.Empty;
        Status = EncounterStatus.Draft;
    }

    public Guid TenantId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ClinicianUserId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public DateTimeOffset EncounterDateUtc { get; private set; }
    public string ChiefComplaint { get; private set; }
    public string Subjective { get; private set; }
    public string Objective { get; private set; }
    public string Assessment { get; private set; }
    public string Plan { get; private set; }
    public string Notes { get; private set; }
    public EncounterStatus Status { get; private set; }
    public DateTimeOffset? SignedAtUtc { get; private set; }
    public ICollection<Vital> Vitals { get; } = [];
    public ICollection<Diagnosis> Diagnoses { get; } = [];
    public ICollection<Prescription> Prescriptions { get; } = [];
    public ICollection<EncounterAuditLog> AuditLogs { get; } = [];

    public void UpdateSoap(
        Guid locationId,
        Guid clinicianUserId,
        DateTimeOffset encounterDateUtc,
        string chiefComplaint,
        string subjective,
        string objective,
        string assessment,
        string plan,
        string? notes)
    {
        EnsureDraft();
        LocationId = locationId;
        ClinicianUserId = clinicianUserId;
        EncounterDateUtc = encounterDateUtc;
        ChiefComplaint = chiefComplaint.Trim();
        Subjective = subjective.Trim();
        Objective = objective.Trim();
        Assessment = assessment.Trim();
        Plan = plan.Trim();
        Notes = notes?.Trim() ?? string.Empty;
    }

    public void Sign(DateTimeOffset signedAtUtc)
    {
        EnsureDraft();
        Status = EncounterStatus.Signed;
        SignedAtUtc = signedAtUtc;
    }

    private void EnsureDraft()
    {
        if (Status != EncounterStatus.Draft)
        {
            throw new InvalidOperationException("Only draft encounters can be changed.");
        }
    }
}
