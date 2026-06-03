using Clinic.Application.Common.Interfaces;
using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed class CheckOutAppointmentCommandHandler(IAppointmentRepository appointments, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CheckOutAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(CheckOutAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointments.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            throw new KeyNotFoundException("Appointment was not found.");
        }

        appointment.CheckOut(dateTimeProvider.UtcNow);
        await appointments.SaveChangesAsync(cancellationToken);
        return appointment.ToDto();
    }
}
