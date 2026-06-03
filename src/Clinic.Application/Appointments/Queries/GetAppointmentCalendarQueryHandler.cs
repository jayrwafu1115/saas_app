using Clinic.Application.Common.Interfaces;
using MediatR;

namespace Clinic.Application.Appointments.Queries;

public sealed class GetAppointmentCalendarQueryHandler(IAppointmentRepository appointments, ICurrentTenant currentTenant)
    : IRequestHandler<GetAppointmentCalendarQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentCalendarQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId ?? currentTenant.TenantId;
        var (fromUtc, toUtc) = GetRange(request.View, request.Date);
        var results = await appointments.ListCalendarAsync(
            tenantId,
            request.LocationId,
            request.DoctorUserId,
            fromUtc,
            toUtc,
            cancellationToken);

        return results.Select(appointment => appointment.ToDto()).ToList();
    }

    private static (DateTimeOffset FromUtc, DateTimeOffset ToUtc) GetRange(string view, DateOnly date)
    {
        var normalizedView = view.Trim().ToLowerInvariant();
        var from = normalizedView switch
        {
            "weekly" => date.AddDays(-1 * (int)date.DayOfWeek),
            "monthly" => new DateOnly(date.Year, date.Month, 1),
            _ => date
        };
        var to = normalizedView switch
        {
            "weekly" => from.AddDays(7),
            "monthly" => from.AddMonths(1),
            _ => from.AddDays(1)
        };

        return (
            new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(to.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
    }
}
