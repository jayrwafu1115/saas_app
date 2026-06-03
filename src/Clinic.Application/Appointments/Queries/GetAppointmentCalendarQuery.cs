using MediatR;

namespace Clinic.Application.Appointments.Queries;

public sealed record GetAppointmentCalendarQuery(
    Guid? TenantId,
    Guid? LocationId,
    Guid? DoctorUserId,
    string View,
    DateOnly Date) : IRequest<IReadOnlyList<AppointmentDto>>;
