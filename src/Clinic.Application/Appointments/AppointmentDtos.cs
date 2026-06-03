using Clinic.Domain.Appointments;

namespace Clinic.Application.Appointments;

public sealed record AppointmentDto(
    Guid Id,
    Guid TenantId,
    Guid LocationId,
    Guid PatientId,
    Guid DoctorUserId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Reason,
    string Notes,
    AppointmentStatus Status,
    DateTimeOffset? CheckedInAtUtc,
    DateTimeOffset? CheckedOutAtUtc,
    DateTimeOffset? CancelledAtUtc);

public sealed record AvailabilityResult(bool IsAvailable, string? Reason);
