namespace Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;

public interface IFileStorageProvider
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType);
    Task<Stream> DownloadAsync(string storagePath);
    Task DeleteAsync(string storagePath);
    Task<string> GetUrlAsync(string storagePath);
}