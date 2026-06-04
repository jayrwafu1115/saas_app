using Clinic.Application.Reporting;
using Clinic.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class ReportingRepository(ApplicationDbContext dbContext) : IReportingRepository
{
    public async Task<DashboardKpiDto> GetKpisAsync(Guid? tenantId, DateOnly from, DateOnly to, decimal visitRevenue, CancellationToken cancellationToken)
    {
        var rangeStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var rangeEnd = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var appointmentQuery = dbContext.Appointments.AsNoTracking()
            .Where(appointment => appointment.StartsAtUtc >= rangeStart && appointment.StartsAtUtc < rangeEnd);
        var patientQuery = dbContext.Patients.AsNoTracking();

        if (tenantId.HasValue)
        {
            appointmentQuery = appointmentQuery.Where(appointment => appointment.TenantId == tenantId.Value);
            patientQuery = patientQuery.Where(patient => patient.TenantId == tenantId.Value);
        }

        var totalPatients = await patientQuery.CountAsync(cancellationToken);
        var newPatients = await patientQuery
            .CountAsync(patient => patient.CreatedAtUtc >= rangeStart && patient.CreatedAtUtc < rangeEnd, cancellationToken);
        var appointments = await appointmentQuery.CountAsync(cancellationToken);
        var completedVisits = await appointmentQuery
            .CountAsync(appointment => appointment.Status == AppointmentStatus.CheckedOut, cancellationToken);
        var activeDoctors = await appointmentQuery
            .Select(appointment => appointment.DoctorUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new DashboardKpiDto(totalPatients, newPatients, appointments, completedVisits * visitRevenue, activeDoctors);
    }

    public async Task<ReportingChartsDto> GetChartsAsync(Guid? tenantId, DateOnly from, DateOnly to, decimal visitRevenue, CancellationToken cancellationToken)
    {
        var rangeStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var rangeEnd = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var appointments = dbContext.Appointments.AsNoTracking()
            .Where(appointment => appointment.StartsAtUtc >= rangeStart && appointment.StartsAtUtc < rangeEnd);
        if (tenantId.HasValue)
        {
            appointments = appointments.Where(appointment => appointment.TenantId == tenantId.Value);
        }

        var dailyRaw = await appointments
            .Where(appointment => appointment.Status == AppointmentStatus.CheckedOut)
            .GroupBy(appointment => appointment.StartsAtUtc.Date)
            .Select(group => new { Date = group.Key, Visits = group.Count() })
            .OrderBy(item => item.Date)
            .ToListAsync(cancellationToken);
        var dailyVisits = dailyRaw
            .Select(item => new DailyVisitDto(DateOnly.FromDateTime(item.Date), item.Visits))
            .ToList();

        var monthlyRaw = await appointments
            .Where(appointment => appointment.Status == AppointmentStatus.CheckedOut)
            .GroupBy(appointment => new { appointment.StartsAtUtc.Year, appointment.StartsAtUtc.Month })
            .Select(group => new { group.Key.Year, group.Key.Month, CompletedVisits = group.Count() })
            .OrderBy(item => item.Year)
            .ThenBy(item => item.Month)
            .ToListAsync(cancellationToken);
        var monthlyRevenue = monthlyRaw
            .Select(item => new MonthlyRevenueDto(item.Year, item.Month, item.CompletedVisits * visitRevenue))
            .ToList();

        var doctorRaw = await appointments
            .GroupBy(appointment => appointment.DoctorUserId)
            .Select(group => new
            {
                DoctorUserId = group.Key,
                Appointments = group.Count(),
                CompletedVisits = group.Count(appointment => appointment.Status == AppointmentStatus.CheckedOut)
            })
            .OrderByDescending(item => item.CompletedVisits)
            .Take(10)
            .ToListAsync(cancellationToken);
        var doctorPerformance = doctorRaw
            .Select(item => new DoctorPerformanceDto(item.DoctorUserId, item.Appointments, item.CompletedVisits, item.CompletedVisits * visitRevenue))
            .ToList();

        var locationRaw = await appointments
            .Join(dbContext.Locations.AsNoTracking(),
                appointment => appointment.LocationId,
                location => location.Id,
                (appointment, location) => new { appointment, location })
            .GroupBy(item => new { item.location.Id, item.location.Name })
            .Select(group => new
            {
                LocationId = group.Key.Id,
                LocationName = group.Key.Name,
                Appointments = group.Count(),
                CompletedVisits = group.Count(item => item.appointment.Status == AppointmentStatus.CheckedOut)
            })
            .OrderByDescending(item => item.CompletedVisits)
            .Take(10)
            .ToListAsync(cancellationToken);
        var locationPerformance = locationRaw
            .Select(item => new LocationPerformanceDto(item.LocationId, item.LocationName, item.Appointments, item.CompletedVisits, item.CompletedVisits * visitRevenue))
            .ToList();

        return new ReportingChartsDto(dailyVisits, monthlyRevenue, doctorPerformance, locationPerformance);
    }
}
