using Clinic.Domain.Appointments;

namespace Clinic.Application.Appointments;

public static class AppointmentMapping
{
    public static AppointmentDto ToDto(this Appointment appointment) =>
        new(
            appointment.Id,
            appointment.TenantId,
            appointment.LocationId,
            appointment.PatientId,
            appointment.DoctorUserId,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Reason,
            appointment.Notes,
            appointment.Status,
            appointment.CheckedInAtUtc,
            appointment.CheckedOutAtUtc,
            appointment.CancelledAtUtc);
}
