namespace Clinic.Application.Appointments;

public interface IAvailabilityService
{
    Task<AvailabilityResult> CheckAvailabilityAsync(
        Guid tenantId,
        Guid locationId,
        Guid doctorUserId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken);
}
