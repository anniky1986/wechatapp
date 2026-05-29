using System.Security.Claims;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.FileManager.Dto;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;

namespace Agility.Zoey.Web.Core.Modules.FileManager.Services;

[ApiDescriptionSettings(Group = "FileManager", Order = 210)]
public class FileFolderService : IDynamicApiController, ITransient
{
    private readonly IRepository<SysFileFolder> _folderRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileFolderService(
        IRepository<SysFileFolder> folderRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _folderRepo = folderRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("api/file-folder/tree")]
    public async Task<List<FileFolderTreeOutput>> GetTree()
    {
        var tenantId = GetCurrentTenantId();
        var folders = await _folderRepo.AsQueryable()
            .Where(f => f.TenantId == tenantId)
            .OrderBy(f => f.Sort)
            .ToListAsync();

        return BuildFolderTree(folders, 0);
    }

    [HttpPost("api/file-folder")]
    public async Task<FileFolderOutput> Add(AddFileFolderInput input)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var folder = new SysFileFolder
        {
            ParentId = input.ParentId,
            Name = input.Name,
            Color = input.Color,
            Icon = input.Icon,
            Sort = input.Sort,
            CreateUserId = userId,
            TenantId = tenantId,
            CreateTime = DateTime.Now
        };

        folder = await _folderRepo.InsertAsync(folder);

        return new FileFolderOutput
        {
            Id = folder.Id,
            ParentId = folder.ParentId,
            Name = folder.Name,
            Color = folder.Color,
            Icon = folder.Icon,
            Sort = folder.Sort,
            CreateTime = folder.CreateTime
        };
    }

    [HttpPut("api/file-folder/{id}")]
    public async Task<FileFolderOutput> Update(long id, UpdateFileFolderInput input)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null)
        {
            throw Oops.Oh("文件夹不存在");
        }

        if (input.ParentId == id)
        {
            throw Oops.Oh("不能将文件夹的父级设置为自身");
        }

        folder.ParentId = input.ParentId;
        folder.Name = input.Name;
        folder.Color = input.Color;
        folder.Icon = input.Icon;
        folder.Sort = input.Sort;
        folder.UpdateTime = DateTime.Now;
        folder.UpdateUserId = GetCurrentUserId();

        await _folderRepo.UpdateAsync(folder);

        return new FileFolderOutput
        {
            Id = folder.Id,
            ParentId = folder.ParentId,
            Name = folder.Name,
            Color = folder.Color,
            Icon = folder.Icon,
            Sort = folder.Sort,
            CreateTime = folder.CreateTime
        };
    }

    [HttpDelete("api/file-folder/{id}")]
    public async Task Delete(long id)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null)
        {
            throw Oops.Oh("文件夹不存在");
        }

        var hasChildren = await _folderRepo.AsQueryable()
            .AnyAsync(f => f.ParentId == id);

        if (hasChildren)
        {
            throw Oops.Oh("存在子文件夹，请先删除子文件夹");
        }

        await _folderRepo.DeleteAsync(id);
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

    private static List<FileFolderTreeOutput> BuildFolderTree(List<SysFileFolder> folders, long parentId)
    {
        return folders
            .Where(f => f.ParentId == parentId)
            .OrderBy(f => f.Sort)
            .Select(f => new FileFolderTreeOutput
            {
                Id = f.Id,
                ParentId = f.ParentId,
                Name = f.Name,
                Color = f.Color,
                Icon = f.Icon,
                Sort = f.Sort,
                Children = BuildFolderTree(folders, f.Id)
            })
            .ToList();
    }
}