using Clinic.Application.Appointments;
using Clinic.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(ApplicationDbContext dbContext) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Appointments.FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> ListCalendarAsync(
        Guid? tenantId,
        Guid? locationId,
        Guid? doctorUserId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Appointments.AsNoTracking()
            .Where(appointment => appointment.StartsAtUtc < toUtc && appointment.EndsAtUtc > fromUtc);

        if (tenantId.HasValue)
        {
            query = query.Where(appointment => appointment.TenantId == tenantId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(appointment => appointment.LocationId == locationId.Value);
        }

        if (doctorUserId.HasValue)
        {
            query = query.Where(appointment => appointment.DoctorUserId == doctorUserId.Value);
        }

        return await query
            .OrderBy(appointment => appointment.StartsAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasConflictAsync(
        Guid tenantId,
        Guid locationId,
        Guid doctorUserId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken) =>
        dbContext.Appointments.AnyAsync(appointment =>
            appointment.TenantId == tenantId
            && appointment.Status != AppointmentStatus.Cancelled
            && (!excludingAppointmentId.HasValue || appointment.Id != excludingAppointmentId.Value)
            && appointment.StartsAtUtc < endsAtUtc
            && appointment.EndsAtUtc > startsAtUtc
            && (appointment.DoctorUserId == doctorUserId || appointment.LocationId == locationId),
            cancellationToken);

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
