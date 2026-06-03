using Clinic.Domain.Clinical;

namespace Clinic.Application.Clinical;

public static class EncounterMapping
{
    public static EncounterDto ToDto(this Encounter encounter) =>
        new(
            encounter.Id,
            encounter.TenantId,
            encounter.LocationId,
            encounter.PatientId,
            encounter.ClinicianUserId,
            encounter.AppointmentId,
            encounter.EncounterDateUtc,
            encounter.ChiefComplaint,
            encounter.Subjective,
            encounter.Objective,
            encounter.Assessment,
            encounter.Plan,
            encounter.Notes,
            encounter.Status,
            encounter.SignedAtUtc);

    public static EncounterDetailDto ToDetailDto(this Encounter encounter) =>
        new(
            encounter.Id,
            encounter.TenantId,
            encounter.LocationId,
            encounter.PatientId,
            encounter.ClinicianUserId,
            encounter.AppointmentId,
            encounter.EncounterDateUtc,
            encounter.ChiefComplaint,
            encounter.Subjective,
            encounter.Objective,
            encounter.Assessment,
            encounter.Plan,
            encounter.Notes,
            encounter.Status,
            encounter.SignedAtUtc,
            encounter.Vitals.Where(vital => !vital.IsDeleted).OrderByDescending(vital => vital.RecordedAtUtc).Select(ToDto).ToList(),
            encounter.Diagnoses.Where(diagnosis => !diagnosis.IsDeleted).OrderBy(diagnosis => diagnosis.Code).Select(ToDto).ToList(),
            encounter.Prescriptions.Where(prescription => !prescription.IsDeleted).OrderBy(prescription => prescription.MedicationName).Select(ToDto).ToList(),
            encounter.AuditLogs.OrderByDescending(log => log.CreatedAtUtc).Select(ToDto).ToList());

    public static VitalDto ToDto(this Vital vital) =>
        new(
            vital.Id,
            vital.EncounterId,
            vital.RecordedAtUtc,
            vital.TemperatureCelsius,
            vital.SystolicBloodPressure,
            vital.DiastolicBloodPressure,
            vital.HeartRate,
            vital.RespiratoryRate,
            vital.OxygenSaturation,
            vital.HeightCm,
            vital.WeightKg,
            vital.Notes);

    public static DiagnosisDto ToDto(this Diagnosis diagnosis) =>
        new(diagnosis.Id, diagnosis.EncounterId, diagnosis.Code, diagnosis.Description, diagnosis.Type);

    public static PrescriptionDto ToDto(this Prescription prescription) =>
        new(prescription.Id, prescription.EncounterId, prescription.MedicationName, prescription.Dosage, prescription.Frequency, prescription.Duration, prescription.Instructions);

    public static EncounterAuditLogDto ToDto(this EncounterAuditLog log) =>
        new(log.Id, log.EncounterId, log.CreatedAtUtc, log.Action, log.Summary, log.ActorUserId);
}
