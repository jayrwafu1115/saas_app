namespace Clinic.Application.Appointments;

public sealed class AvailabilityService(IAppointmentRepository appointments) : IAvailabilityService
{
    public async Task<AvailabilityResult> CheckAvailabilityAsync(
        Guid tenantId,
        Guid locationId,
        Guid doctorUserId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            return new AvailabilityResult(false, "Appointment end time must be after start time.");
        }

        if (startsAtUtc.Minute % 5 != 0 || endsAtUtc.Minute % 5 != 0)
        {
            return new AvailabilityResult(false, "Appointments must start and end on five-minute intervals.");
        }

        var localStart = startsAtUtc.ToOffset(TimeSpan.Zero);
        var localEnd = endsAtUtc.ToOffset(TimeSpan.Zero);
        if (localStart.Hour < 8 || localEnd.Hour > 18 || localEnd is { Hour: 18, Minute: > 0 })
        {
            return new AvailabilityResult(false, "Appointments must be between 08:00 and 18:00 UTC.");
        }

        var hasConflict = await appointments.HasConflictAsync(
            tenantId,
            locationId,
            doctorUserId,
            startsAtUtc,
            endsAtUtc,
            excludingAppointmentId,
            cancellationToken);

        return hasConflict
            ? new AvailabilityResult(false, "The doctor or location already has an appointment at this time.")
            : new AvailabilityResult(true, null);
    }
}
