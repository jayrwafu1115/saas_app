using Clinic.Application.Common.Interfaces;
using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed class CancelAppointmentCommandHandler(IAppointmentRepository appointments, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CancelAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointments.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            throw new KeyNotFoundException("Appointment was not found.");
        }

        appointment.Cancel(dateTimeProvider.UtcNow);
        await appointments.SaveChangesAsync(cancellationToken);
        return appointment.ToDto();
    }
}
