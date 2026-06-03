using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed record CheckInAppointmentCommand(Guid Id) : IRequest<AppointmentDto>;
