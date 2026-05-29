namespace Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;

public class AliyunOSSProvider : IFileStorageProvider
{
    public Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        throw new NotImplementedException("Aliyun OSS integration not yet implemented");
    }

    public Task<Stream> DownloadAsync(string storagePath)
    {
        throw new NotImplementedException("Aliyun OSS integration not yet implemented");
    }

    public Task DeleteAsync(string storagePath)
    {
        throw new NotImplementedException("Aliyun OSS integration not yet implemented");
    }

    public Task<string> GetUrlAsync(string storagePath)
    {
        throw new NotImplementedException("Aliyun OSS integration not yet implemented");
    }
}