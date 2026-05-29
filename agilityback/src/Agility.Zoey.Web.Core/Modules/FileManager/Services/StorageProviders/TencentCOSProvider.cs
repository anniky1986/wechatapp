namespace Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;

public class TencentCOSProvider : IFileStorageProvider
{
    public Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        throw new NotImplementedException("Tencent COS integration not yet implemented");
    }

    public Task<Stream> DownloadAsync(string storagePath)
    {
        throw new NotImplementedException("Tencent COS integration not yet implemented");
    }

    public Task DeleteAsync(string storagePath)
    {
        throw new NotImplementedException("Tencent COS integration not yet implemented");
    }

    public Task<string> GetUrlAsync(string storagePath)
    {
        throw new NotImplementedException("Tencent COS integration not yet implemented");
    }
}