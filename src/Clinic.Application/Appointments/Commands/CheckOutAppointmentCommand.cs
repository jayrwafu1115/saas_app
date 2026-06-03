using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed record CheckOutAppointmentCommand(Guid Id) : IRequest<AppointmentDto>;
