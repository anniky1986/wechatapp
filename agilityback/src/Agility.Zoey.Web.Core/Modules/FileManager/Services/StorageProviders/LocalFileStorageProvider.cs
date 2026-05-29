using Microsoft.Extensions.Configuration;

namespace Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;

public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly string _basePath;

    public LocalFileStorageProvider(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:LocalPath"] ?? "uploads";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _basePath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        var relativePath = Path.Combine(DateTime.Now.ToString("yyyy/MM/dd"), fileName);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _basePath, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir) && dir != null)
        {
            Directory.CreateDirectory(dir);
        }

        using var fileStream = new FileStream(fullPath, FileMode.Create);
        stream.CopyTo(fileStream);

        return Task.FromResult(relativePath);
    }

    public Task<Stream> DownloadAsync(string storagePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _basePath, storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found", fullPath);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _basePath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string storagePath)
    {
        var url = $"/{_basePath}/{storagePath}".Replace("\\", "/");
        return Task.FromResult(url);
    }
}