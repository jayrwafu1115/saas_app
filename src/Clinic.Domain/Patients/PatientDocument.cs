using Clinic.Domain.Common;

namespace Clinic.Domain.Patients;

public sealed class PatientDocument : BaseEntity
{
    private PatientDocument()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        ObjectKey = string.Empty;
        Patient = null!;
    }

    public PatientDocument(
        Guid patientId,
        string fileName,
        string contentType,
        long sizeBytes,
        string objectKey,
        DateTimeOffset uploadedAtUtc)
    {
        PatientId = patientId;
        FileName = fileName.Trim();
        ContentType = contentType.Trim();
        SizeBytes = sizeBytes;
        ObjectKey = objectKey;
        UploadedAtUtc = uploadedAtUtc;
        Patient = null!;
    }

    public Guid PatientId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string ObjectKey { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
    public Patient Patient { get; private set; }
}
