using Clinic.Domain.Patients;

namespace Clinic.Application.Patients;

public static class PatientMapping
{
    public static PatientDto ToDto(this Patient patient) =>
        new(
            patient.Id,
            patient.TenantId,
            patient.LocationId,
            patient.MedicalRecordNumber,
            patient.FirstName,
            patient.MiddleName,
            patient.LastName,
            patient.BirthDate,
            patient.Gender,
            patient.Email,
            patient.Phone,
            patient.Address);

    public static PatientDetailDto ToDetailDto(this Patient patient) =>
        new(
            patient.Id,
            patient.TenantId,
            patient.LocationId,
            patient.MedicalRecordNumber,
            patient.FirstName,
            patient.MiddleName,
            patient.LastName,
            patient.BirthDate,
            patient.Gender,
            patient.Email,
            patient.Phone,
            patient.Address,
            patient.Contacts.Where(contact => !contact.IsDeleted).Select(contact => contact.ToDto()).ToList(),
            patient.Documents.Where(document => !document.IsDeleted).Select(document => document.ToDto()).ToList());

    public static PatientContactDto ToDto(this PatientContact contact) =>
        new(contact.Id, contact.PatientId, contact.Name, contact.Relationship, contact.Email, contact.Phone, contact.IsPrimary);

    public static PatientDocumentDto ToDto(this PatientDocument document) =>
        new(document.Id, document.PatientId, document.FileName, document.ContentType, document.SizeBytes, document.ObjectKey, document.UploadedAtUtc);
}
