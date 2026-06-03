using Clinic.Domain.Clinical;

namespace Clinic.Application.Clinical;

public sealed record EncounterDto(
    Guid Id,
    Guid TenantId,
    Guid LocationId,
    Guid PatientId,
    Guid ClinicianUserId,
    Guid? AppointmentId,
    DateTimeOffset EncounterDateUtc,
    string ChiefComplaint,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string Notes,
    EncounterStatus Status,
    DateTimeOffset? SignedAtUtc);

public sealed record EncounterDetailDto(
    Guid Id,
    Guid TenantId,
    Guid LocationId,
    Guid PatientId,
    Guid ClinicianUserId,
    Guid? AppointmentId,
    DateTimeOffset EncounterDateUtc,
    string ChiefComplaint,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string Notes,
    EncounterStatus Status,
    DateTimeOffset? SignedAtUtc,
    IReadOnlyList<VitalDto> Vitals,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<PrescriptionDto> Prescriptions,
    IReadOnlyList<EncounterAuditLogDto> AuditLogs);

public sealed record VitalDto(
    Guid Id,
    Guid EncounterId,
    DateTimeOffset RecordedAtUtc,
    decimal? TemperatureCelsius,
    int? SystolicBloodPressure,
    int? DiastolicBloodPressure,
    int? HeartRate,
    int? RespiratoryRate,
    int? OxygenSaturation,
    decimal? HeightCm,
    decimal? WeightKg,
    string Notes);

public sealed record DiagnosisDto(Guid Id, Guid EncounterId, string Code, string Description, string Type);
public sealed record PrescriptionDto(Guid Id, Guid EncounterId, string MedicationName, string Dosage, string Frequency, string Duration, string Instructions);
public sealed record EncounterAuditLogDto(Guid Id, Guid EncounterId, DateTimeOffset OccurredAtUtc, string Action, string Summary, string ActorUserId);
public sealed record EncounterTimelineEventDto(DateTimeOffset OccurredAtUtc, string Type, string Title, string Description);
