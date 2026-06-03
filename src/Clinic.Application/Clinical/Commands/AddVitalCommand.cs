using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed record AddVitalCommand(
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
    string? Notes) : IRequest<VitalDto>;
