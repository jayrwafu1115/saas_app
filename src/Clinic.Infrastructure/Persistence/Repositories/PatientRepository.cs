using Clinic.Application.Common.Models;
using Clinic.Application.Patients;
using Clinic.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository(ApplicationDbContext dbContext) : IPatientRepository
{
    public async Task<PagedResult<Patient>> SearchAsync(
        Guid? tenantId,
        Guid? locationId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Patients.AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(patient => patient.TenantId == tenantId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(patient => patient.LocationId == locationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(patient =>
                patient.MedicalRecordNumber.ToLower().Contains(normalizedSearch)
                || patient.FirstName.ToLower().Contains(normalizedSearch)
                || patient.LastName.ToLower().Contains(normalizedSearch)
                || patient.Email.ToLower().Contains(normalizedSearch)
                || patient.Phone.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Patient>(items, pageNumber, pageSize, totalCount);
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Patients
            .Include(patient => patient.Contacts)
            .Include(patient => patient.Documents)
            .FirstOrDefaultAsync(patient => patient.Id == id, cancellationToken);

    public Task<PatientContact?> GetContactByIdAsync(Guid patientId, Guid contactId, CancellationToken cancellationToken) =>
        dbContext.PatientContacts.FirstOrDefaultAsync(
            contact => contact.PatientId == patientId && contact.Id == contactId,
            cancellationToken);

    public Task<bool> MedicalRecordNumberExistsAsync(
        Guid tenantId,
        string medicalRecordNumber,
        Guid? excludingPatientId,
        CancellationToken cancellationToken)
    {
        var normalizedMedicalRecordNumber = medicalRecordNumber.Trim().ToUpperInvariant();
        return dbContext.Patients.AnyAsync(
            patient => patient.TenantId == tenantId
                && patient.MedicalRecordNumber == normalizedMedicalRecordNumber
                && (!excludingPatientId.HasValue || patient.Id != excludingPatientId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddContactAsync(PatientContact contact, CancellationToken cancellationToken)
    {
        dbContext.PatientContacts.Add(contact);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddDocumentAsync(PatientDocument document, CancellationToken cancellationToken)
    {
        dbContext.PatientDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
