using Clinic.Application.Common.Models;
using Clinic.Domain.Patients;

namespace Clinic.Application.Patients;

public interface IPatientRepository
{
    Task<PagedResult<Patient>> SearchAsync(
        Guid? tenantId,
        Guid? locationId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PatientContact?> GetContactByIdAsync(Guid patientId, Guid contactId, CancellationToken cancellationToken);
    Task<bool> MedicalRecordNumberExistsAsync(Guid tenantId, string medicalRecordNumber, Guid? excludingPatientId, CancellationToken cancellationToken);
    Task AddAsync(Patient patient, CancellationToken cancellationToken);
    Task AddContactAsync(PatientContact contact, CancellationToken cancellationToken);
    Task AddDocumentAsync(PatientDocument document, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
