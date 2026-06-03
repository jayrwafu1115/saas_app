using Clinic.Domain.Appointments;

namespace Clinic.Application.Appointments;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Appointment>> ListCalendarAsync(Guid? tenantId, Guid? locationId, Guid? doctorUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
    Task<bool> HasConflictAsync(Guid tenantId, Guid locationId, Guid doctorUserId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, Guid? excludingAppointmentId, CancellationToken cancellationToken);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
