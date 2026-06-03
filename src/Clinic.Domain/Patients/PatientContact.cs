using Clinic.Domain.Common;

namespace Clinic.Domain.Patients;

public sealed class PatientContact : BaseEntity
{
    private PatientContact()
    {
        Name = string.Empty;
        Relationship = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Patient = null!;
    }

    public PatientContact(Guid patientId, string name, string relationship, string email, string phone, bool isPrimary)
    {
        PatientId = patientId;
        Name = name.Trim();
        Relationship = relationship.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        IsPrimary = isPrimary;
        Patient = null!;
    }

    public Guid PatientId { get; private set; }
    public string Name { get; private set; }
    public string Relationship { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public bool IsPrimary { get; private set; }
    public Patient Patient { get; private set; }

    public void Update(string name, string relationship, string email, string phone, bool isPrimary)
    {
        Name = name.Trim();
        Relationship = relationship.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        IsPrimary = isPrimary;
    }
}
