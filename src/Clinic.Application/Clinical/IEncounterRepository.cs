using Clinic.Domain.Clinical;

namespace Clinic.Application.Clinical;

public interface IEncounterRepository
{
    Task<IReadOnlyList<Encounter>> ListByPatientAsync(Guid patientId, CancellationToken cancellationToken);
    Task<Encounter?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Encounter encounter, CancellationToken cancellationToken);
    Task AddVitalAsync(Vital vital, CancellationToken cancellationToken);
    Task AddDiagnosisAsync(Diagnosis diagnosis, CancellationToken cancellationToken);
    Task AddPrescriptionAsync(Prescription prescription, CancellationToken cancellationToken);
    Task AddAuditLogAsync(EncounterAuditLog auditLog, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
