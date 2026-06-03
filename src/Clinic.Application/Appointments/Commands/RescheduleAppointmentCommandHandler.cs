using Clinic.Application.Locations;
using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed class RescheduleAppointmentCommandHandler(
    IAppointmentRepository appointments,
    IAvailabilityService availability,
    ILocationRepository locations)
    : IRequestHandler<RescheduleAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointments.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            throw new KeyNotFoundException("Appointment was not found.");
        }

        var locationExists = (await locations.ListAsync(appointment.TenantId, cancellationToken))
            .Any(location => location.Id == request.LocationId);
        if (!locationExists)
        {
            throw new InvalidOperationException("Location does not exist for this tenant.");
        }

        var result = await availability.CheckAvailabilityAsync(
            appointment.TenantId,
            request.LocationId,
            request.DoctorUserId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            appointment.Id,
            cancellationToken);
        if (!result.IsAvailable)
        {
            throw new InvalidOperationException(result.Reason);
        }

        appointment.Reschedule(request.LocationId, request.DoctorUserId, request.StartsAtUtc, request.EndsAtUtc);
        await appointments.SaveChangesAsync(cancellationToken);
        return appointment.ToDto();
    }
}
