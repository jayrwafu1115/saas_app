using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed record RescheduleAppointmentCommand(
    Guid Id,
    Guid LocationId,
    Guid DoctorUserId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : IRequest<AppointmentDto>;
