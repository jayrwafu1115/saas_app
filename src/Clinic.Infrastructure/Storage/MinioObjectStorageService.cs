using Clinic.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Clinic.Infrastructure.Storage;

public sealed class MinioObjectStorageService : IObjectStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioStorageOptions _options;

    public MinioObjectStorageService(IOptions<MinioStorageOptions> options)
    {
        _options = options.Value;
        _client = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl)
            .Build();
    }

    public async Task<StoredObject> UploadAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken)
    {
        var bucketExists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName),
            cancellationToken);
        if (!bucketExists)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.BucketName),
                cancellationToken);
        }

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithStreamData(content)
                .WithObjectSize(contentLength)
                .WithContentType(contentType),
            cancellationToken);

        return new StoredObject(objectKey);
    }
}
