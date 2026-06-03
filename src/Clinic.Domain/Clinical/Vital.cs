using Clinic.Domain.Common;

namespace Clinic.Domain.Clinical;

public sealed class Vital : BaseEntity
{
    private Vital()
    {
        Notes = string.Empty;
    }

    public Vital(
        Guid tenantId,
        Guid encounterId,
        DateTimeOffset recordedAtUtc,
        decimal? temperatureCelsius,
        int? systolicBloodPressure,
        int? diastolicBloodPressure,
        int? heartRate,
        int? respiratoryRate,
        int? oxygenSaturation,
        decimal? heightCm,
        decimal? weightKg,
        string? notes)
    {
        TenantId = tenantId;
        EncounterId = encounterId;
        RecordedAtUtc = recordedAtUtc;
        TemperatureCelsius = temperatureCelsius;
        SystolicBloodPressure = systolicBloodPressure;
        DiastolicBloodPressure = diastolicBloodPressure;
        HeartRate = heartRate;
        RespiratoryRate = respiratoryRate;
        OxygenSaturation = oxygenSaturation;
        HeightCm = heightCm;
        WeightKg = weightKg;
        Notes = notes?.Trim() ?? string.Empty;
    }

    public Guid TenantId { get; private set; }
    public Guid EncounterId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
    public decimal? TemperatureCelsius { get; private set; }
    public int? SystolicBloodPressure { get; private set; }
    public int? DiastolicBloodPressure { get; private set; }
    public int? HeartRate { get; private set; }
    public int? RespiratoryRate { get; private set; }
    public int? OxygenSaturation { get; private set; }
    public decimal? HeightCm { get; private set; }
    public decimal? WeightKg { get; private set; }
    public string Notes { get; private set; }
}
