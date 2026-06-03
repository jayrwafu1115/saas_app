namespace Clinic.Infrastructure.Storage;

public sealed class MinioStorageOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin123";
    public string BucketName { get; set; } = "patient-documents";
    public bool UseSsl { get; set; }
}
