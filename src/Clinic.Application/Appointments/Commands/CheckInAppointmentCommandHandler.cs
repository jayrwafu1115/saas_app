using Clinic.Application.Common.Interfaces;
using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed class CheckInAppointmentCommandHandler(IAppointmentRepository appointments, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CheckInAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(CheckInAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointments.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            throw new KeyNotFoundException("Appointment was not found.");
        }

        appointment.CheckIn(dateTimeProvider.UtcNow);
        await appointments.SaveChangesAsync(cancellationToken);
        return appointment.ToDto();
    }
}
