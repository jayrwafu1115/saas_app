using Clinic.Application.Clinical;
using Clinic.Domain.Clinical;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class EncounterRepository(ApplicationDbContext dbContext) : IEncounterRepository
{
    public async Task<IReadOnlyList<Encounter>> ListByPatientAsync(Guid patientId, CancellationToken cancellationToken) =>
        await dbContext.Encounters
            .AsNoTracking()
            .Include(encounter => encounter.Diagnoses)
            .Include(encounter => encounter.Prescriptions)
            .Where(encounter => encounter.PatientId == patientId)
            .OrderByDescending(encounter => encounter.EncounterDateUtc)
            .ToListAsync(cancellationToken);

    public Task<Encounter?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Encounters
            .Include(encounter => encounter.Vitals)
            .Include(encounter => encounter.Diagnoses)
            .Include(encounter => encounter.Prescriptions)
            .Include(encounter => encounter.AuditLogs)
            .FirstOrDefaultAsync(encounter => encounter.Id == id, cancellationToken);

    public async Task AddAsync(Encounter encounter, CancellationToken cancellationToken)
    {
        dbContext.Encounters.Add(encounter);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddVitalAsync(Vital vital, CancellationToken cancellationToken)
    {
        dbContext.Vitals.Add(vital);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddDiagnosisAsync(Diagnosis diagnosis, CancellationToken cancellationToken)
    {
        dbContext.Diagnoses.Add(diagnosis);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPrescriptionAsync(Prescription prescription, CancellationToken cancellationToken)
    {
        dbContext.Prescriptions.Add(prescription);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAuditLogAsync(EncounterAuditLog auditLog, CancellationToken cancellationToken)
    {
        dbContext.EncounterAuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
