using Clinic.Domain.Common;

namespace Clinic.Domain.Clinical;

public sealed class Prescription : BaseEntity
{
    private Prescription()
    {
        MedicationName = string.Empty;
        Dosage = string.Empty;
        Frequency = string.Empty;
        Duration = string.Empty;
        Instructions = string.Empty;
    }

    public Prescription(
        Guid tenantId,
        Guid encounterId,
        string medicationName,
        string dosage,
        string frequency,
        string duration,
        string? instructions)
    {
        TenantId = tenantId;
        EncounterId = encounterId;
        MedicationName = medicationName.Trim();
        Dosage = dosage.Trim();
        Frequency = frequency.Trim();
        Duration = duration.Trim();
        Instructions = instructions?.Trim() ?? string.Empty;
    }

    public Guid TenantId { get; private set; }
    public Guid EncounterId { get; private set; }
    public string MedicationName { get; private set; }
    public string Dosage { get; private set; }
    public string Frequency { get; private set; }
    public string Duration { get; private set; }
    public string Instructions { get; private set; }
}
