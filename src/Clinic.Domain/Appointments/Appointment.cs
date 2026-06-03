using Clinic.Domain.Common;

namespace Clinic.Domain.Appointments;

public sealed class Appointment : BaseEntity
{
    private Appointment()
    {
        Reason = string.Empty;
        Notes = string.Empty;
    }

    public Appointment(
        Guid tenantId,
        Guid locationId,
        Guid patientId,
        Guid doctorUserId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string reason,
        string? notes)
    {
        TenantId = tenantId;
        LocationId = locationId;
        PatientId = patientId;
        DoctorUserId = doctorUserId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Reason = reason.Trim();
        Notes = notes?.Trim() ?? string.Empty;
        Status = AppointmentStatus.Scheduled;
    }

    public Guid TenantId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorUserId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public string Reason { get; private set; }
    public string Notes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTimeOffset? CheckedInAtUtc { get; private set; }
    public DateTimeOffset? CheckedOutAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public void Reschedule(Guid locationId, Guid doctorUserId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        EnsureNotCancelled();
        LocationId = locationId;
        DoctorUserId = doctorUserId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        EnsureNotCancelled();
        Status = AppointmentStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
    }

    public void CheckIn(DateTimeOffset checkedInAtUtc)
    {
        EnsureNotCancelled();
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can be checked in.");
        }

        Status = AppointmentStatus.CheckedIn;
        CheckedInAtUtc = checkedInAtUtc;
    }

    public void CheckOut(DateTimeOffset checkedOutAtUtc)
    {
        EnsureNotCancelled();
        if (Status != AppointmentStatus.CheckedIn)
        {
            throw new InvalidOperationException("Only checked-in appointments can be checked out.");
        }

        Status = AppointmentStatus.CheckedOut;
        CheckedOutAtUtc = checkedOutAtUtc;
    }

    private void EnsureNotCancelled()
    {
        if (Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled appointments cannot be changed.");
        }
    }
}
