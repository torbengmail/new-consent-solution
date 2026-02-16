using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;

namespace PrivacyConsent.Infrastructure.Storage;

public class GcsFileStorageService : IFileStorageService
{
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;
    private readonly ILogger<GcsFileStorageService> _logger;

    public GcsFileStorageService(StorageClient storageClient, UrlSigner urlSigner, ILogger<GcsFileStorageService> logger)
    {
        _storageClient = storageClient;
        _urlSigner = urlSigner;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string bucketName, string key, Stream content, string contentType)
    {
        var obj = await _storageClient.UploadObjectAsync(bucketName, key, contentType, content);
        _logger.LogInformation("Uploaded file to gs://{Bucket}/{Key}", bucketName, key);
        return $"gs://{bucketName}/{key}";
    }

    public async Task<string> GenerateUploadLinkAsync(string bucketName, string key, TimeSpan expiration)
    {
        var url = await _urlSigner.SignAsync(bucketName, key, expiration, HttpMethod.Put);
        return url;
    }

    public async Task<Stream?> DownloadFileAsync(string bucketName, string key)
    {
        var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(bucketName, key, stream);
        stream.Position = 0;
        return stream;
    }
}
