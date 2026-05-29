using System.Security.Claims;
using System.Security.Cryptography;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.FileManager.Dto;
using Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Agility.Zoey.Web.Core.Modules.FileManager.Services;

[ApiDescriptionSettings(Group = "FileManager", Order = 200)]
public class FileStorageService : IDynamicApiController, ITransient
{
    private readonly IRepository<SysFile> _fileRepo;
    private readonly IRepository<SysFileFolder> _folderRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public FileStorageService(
        IRepository<SysFile> fileRepo,
        IRepository<SysFileFolder> folderRepo,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _fileRepo = fileRepo;
        _folderRepo = folderRepo;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    [HttpPost("api/file/upload")]
    [Permission(PermissionConsts.FileUpload)]
    public async Task<FileOutput> Upload(IFormFile file, string? category, long? folderId)
    {
        if (file == null || file.Length == 0)
        {
            throw Oops.Oh("文件不能为空");
        }

        if (file.Length > SystemConsts.MaxFileSize)
        {
            throw Oops.Oh($"文件大小不能超过 {SystemConsts.MaxFileSize / 1024 / 1024}MB");
        }

        var userId = GetCurrentUserId();
        var tenantId = GetCurrentTenantId();
        var provider = _serviceProvider.GetRequiredService<LocalFileStorageProvider>();

        var fileId = Guid.NewGuid().ToString("N");
        var ext = Path.GetExtension(file.FileName);
        var storageName = $"{fileId}{ext}";

        using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        var fileHash = ComputeHash(ms);
        ms.Position = 0;

        var storagePath = await provider.UploadAsync(ms, storageName, file.ContentType);

        var sysFile = new SysFile
        {
            FileId = fileId,
            OriginalName = file.FileName,
            StorageName = storageName,
            ContentType = file.ContentType,
            Size = file.Length,
            FileHash = fileHash,
            StoragePath = storagePath,
            StorageProvider = (int)Core.Enums.StorageProvider.Local,
            Category = category,
            FolderId = folderId,
            UploadUserId = userId,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = userId
        };

        sysFile = await _fileRepo.InsertAsync(sysFile);

        var url = await provider.GetUrlAsync(storagePath);
        return MapToOutput(sysFile, url);
    }

    [HttpPost("api/file/multi-upload")]
    [Permission(PermissionConsts.FileUpload)]
    public async Task<List<FileOutput>> MultiUpload(List<IFormFile> files, string? category, long? folderId)
    {
        var results = new List<FileOutput>();
        foreach (var file in files)
        {
            var result = await Upload(file, category, folderId);
            results.Add(result);
        }
        return results;
    }

    [HttpGet("api/file/page")]
    [Permission(PermissionConsts.FileView)]
    public async Task<PageResult<FileOutput>> GetPage(FilePageInput input)
    {
        var tenantId = GetCurrentTenantId();
        var query = _fileRepo.AsQueryable()
            .Where(f => f.TenantId == tenantId);

        if (!string.IsNullOrEmpty(input.OriginalName))
        {
            query = query.Where(f => f.OriginalName != null && f.OriginalName.Contains(input.OriginalName));
        }

        if (!string.IsNullOrEmpty(input.Category))
        {
            query = query.Where(f => f.Category == input.Category);
        }

        if (input.FolderId.HasValue)
        {
            query = query.Where(f => f.FolderId == input.FolderId.Value);
        }

        var page = input.Page < 1 ? 1 : input.Page;
        var pageSize = input.PageSize < 1 ? SystemConsts.DefaultPageSize : input.PageSize;
        if (pageSize > SystemConsts.MaxPageSize) pageSize = SystemConsts.MaxPageSize;

        query = query.OrderBy(f => f.Id, OrderByType.Desc);

        var totalCount = await query.CountAsync();
        var files = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var provider = _serviceProvider.GetRequiredService<LocalFileStorageProvider>();

        var items = new List<FileOutput>();
        foreach (var file in files)
        {
            var url = await provider.GetUrlAsync(file.StoragePath);
            var output = MapToOutput(file, url);
            items.Add(output);
        }

        return new PageResult<FileOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/file/{id}")]
    [Permission(PermissionConsts.FileView)]
    public async Task<FileOutput> Get(long id)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null)
        {
            throw Oops.Oh("文件不存在");
        }

        var provider = _serviceProvider.GetRequiredService<LocalFileStorageProvider>();
        var url = await provider.GetUrlAsync(file.StoragePath);

        return MapToOutput(file, url);
    }

    [HttpGet("api/file/download/{id}")]
    [Permission(PermissionConsts.FileDownload)]
    public async Task<IActionResult> Download(long id)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null)
        {
            throw Oops.Oh("文件不存在");
        }

        var provider = _serviceProvider.GetRequiredService<LocalFileStorageProvider>();
        var stream = await provider.DownloadAsync(file.StoragePath);

        return new FileStreamResult(stream, file.ContentType ?? "application/octet-stream")
        {
            FileDownloadName = file.OriginalName ?? file.StorageName
        };
    }

    [HttpDelete("api/file/{id}")]
    [Permission(PermissionConsts.FileDelete)]
    public async Task Delete(long id)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null)
        {
            throw Oops.Oh("文件不存在");
        }

        var provider = _serviceProvider.GetRequiredService<LocalFileStorageProvider>();
        await provider.DeleteAsync(file.StoragePath);

        await _fileRepo.DeleteAsync(id);
    }

    [HttpDelete("api/file/batch")]
    [Permission(PermissionConsts.FileDelete)]
    public async Task BatchDelete(List<long> ids)
    {
        var provider = _serviceProvider.GetRequiredService<LocalFileStorageProvider>();

        foreach (var id in ids)
        {
            var file = await _fileRepo.GetByIdAsync(id);
            if (file != null)
            {
                await provider.DeleteAsync(file.StoragePath);
            }
        }

        await _fileRepo.DeleteAsync(f => ids.Contains(f.Id));
    }

    private long GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && long.TryParse(claim, out var id) ? id : 0;
    }

    private long GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
        return claim != null && long.TryParse(claim, out var id) ? id : 0;
    }

    private static string ComputeHash(Stream stream)
    {
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hashBytes);
    }

    private FileOutput MapToOutput(SysFile file, string url)
    {
        return new FileOutput
        {
            Id = file.Id,
            FileId = file.FileId,
            OriginalName = file.OriginalName,
            ContentType = file.ContentType,
            Size = file.Size,
            FileHash = file.FileHash,
            Category = file.Category,
            FolderId = file.FolderId,
            StorageProvider = file.StorageProvider,
            UploadUserId = file.UploadUserId,
            Url = url,
            CreateTime = file.CreateTime
        };
    }
}