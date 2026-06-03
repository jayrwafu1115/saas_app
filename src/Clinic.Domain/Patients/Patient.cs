using Clinic.Domain.Common;

namespace Clinic.Domain.Patients;

public sealed class Patient : BaseEntity
{
    private Patient()
    {
        MedicalRecordNumber = string.Empty;
        FirstName = string.Empty;
        MiddleName = string.Empty;
        LastName = string.Empty;
        Gender = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Address = string.Empty;
    }

    public Patient(
        Guid tenantId,
        Guid locationId,
        string medicalRecordNumber,
        string firstName,
        string? middleName,
        string lastName,
        DateOnly birthDate,
        string gender,
        string email,
        string phone,
        string address)
    {
        TenantId = tenantId;
        LocationId = locationId;
        MedicalRecordNumber = medicalRecordNumber.Trim().ToUpperInvariant();
        FirstName = firstName.Trim();
        MiddleName = middleName?.Trim() ?? string.Empty;
        LastName = lastName.Trim();
        BirthDate = birthDate;
        Gender = gender.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        Address = address.Trim();
    }

    public Guid TenantId { get; private set; }
    public Guid LocationId { get; private set; }
    public string MedicalRecordNumber { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string Gender { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string Address { get; private set; }
    public ICollection<PatientContact> Contacts { get; } = [];
    public ICollection<PatientDocument> Documents { get; } = [];

    public void Update(
        Guid locationId,
        string medicalRecordNumber,
        string firstName,
        string? middleName,
        string lastName,
        DateOnly birthDate,
        string gender,
        string email,
        string phone,
        string address)
    {
        LocationId = locationId;
        MedicalRecordNumber = medicalRecordNumber.Trim().ToUpperInvariant();
        FirstName = firstName.Trim();
        MiddleName = middleName?.Trim() ?? string.Empty;
        LastName = lastName.Trim();
        BirthDate = birthDate;
        Gender = gender.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        Address = address.Trim();
    }
}
