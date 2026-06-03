using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed record CancelAppointmentCommand(Guid Id) : IRequest<AppointmentDto>;
