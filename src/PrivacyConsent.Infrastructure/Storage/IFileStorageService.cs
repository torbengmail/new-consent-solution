namespace PrivacyConsent.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string bucketName, string key, Stream content, string contentType);
    Task<string> GenerateUploadLinkAsync(string bucketName, string key, TimeSpan expiration);
    Task<Stream?> DownloadFileAsync(string bucketName, string key);
}
