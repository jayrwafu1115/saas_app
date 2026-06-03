namespace Clinic.Application.Common.Interfaces;

public interface IObjectStorageService
{
    Task<StoredObject> UploadAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken);
}

public sealed record StoredObject(string ObjectKey);
