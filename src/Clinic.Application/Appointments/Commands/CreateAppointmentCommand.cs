using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed record CreateAppointmentCommand(
    Guid TenantId,
    Guid LocationId,
    Guid PatientId,
    Guid DoctorUserId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Reason,
    string? Notes) : IRequest<AppointmentDto>;
