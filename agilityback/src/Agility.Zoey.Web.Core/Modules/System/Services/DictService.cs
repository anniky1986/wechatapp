using System.Security.Claims;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 150)]
public class DictService : IDynamicApiController, ITransient
{
    private readonly IRepository<Dict> _dictRepo;
    private readonly IRepository<DictItem> _dictItemRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DictService(IRepository<Dict> dictRepo, IRepository<DictItem> dictItemRepo, IHttpContextAccessor httpContextAccessor)
    {
        _dictRepo = dictRepo;
        _dictItemRepo = dictItemRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("api/dict/page")]
    [Permission(PermissionConsts.DictView)]
    public async Task<PageResult<DictOutput>> GetPage(DictPageInput input)
    {
        var query = _dictRepo.AsQueryable();

        if (!string.IsNullOrEmpty(input.Name))
        {
            query = query.Where(d => d.Name.Contains(input.Name));
        }

        if (!string.IsNullOrEmpty(input.Code))
        {
            query = query.Where(d => d.Code.Contains(input.Code));
        }

        var page = input.Page < 1 ? 1 : input.Page;
        var pageSize = input.PageSize < 1 ? SystemConsts.DefaultPageSize : input.PageSize;
        if (pageSize > SystemConsts.MaxPageSize) pageSize = SystemConsts.MaxPageSize;

        query = query.OrderBy(d => d.Id, OrderByType.Desc);

        var totalCount = await query.CountAsync();
        var dicts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = dicts.Select(d => new DictOutput
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            Remark = d.Remark,
            CreateTime = d.CreateTime
        }).ToList();

        return new PageResult<DictOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/dict/{id}")]
    [Permission(PermissionConsts.DictView)]
    public async Task<DictOutput> Get(long id)
    {
        var dict = await _dictRepo.GetByIdAsync(id);
        if (dict == null)
        {
            throw Oops.Oh("字典不存在");
        }

        return new DictOutput
        {
            Id = dict.Id,
            Name = dict.Name,
            Code = dict.Code,
            Remark = dict.Remark,
            CreateTime = dict.CreateTime
        };
    }

    [HttpPost("api/dict")]
    [Permission(PermissionConsts.DictAdd)]
    public async Task<DictOutput> Add(AddDictInput input)
    {
        var tenantId = GetCurrentTenantId();
        var exists = await _dictRepo.AsQueryable()
            .AnyAsync(d => d.Code == input.Code && d.TenantId == tenantId);
        if (exists)
        {
            throw Oops.Oh("字典编码已存在");
        }

        var dict = new Dict
        {
            Name = input.Name,
            Code = input.Code,
            Remark = input.Remark,
            TenantId = tenantId,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        dict = await _dictRepo.InsertAsync(dict);
        return await Get(dict.Id);
    }

    [HttpPut("api/dict/{id}")]
    [Permission(PermissionConsts.DictEdit)]
    public async Task<DictOutput> Update(long id, UpdateDictInput input)
    {
        var dict = await _dictRepo.GetByIdAsync(id);
        if (dict == null)
        {
            throw Oops.Oh("字典不存在");
        }

        var exists = await _dictRepo.AsQueryable()
            .AnyAsync(d => d.Code == input.Code && d.Id != id);
        if (exists)
        {
            throw Oops.Oh("字典编码已存在");
        }

        dict.Name = input.Name;
        dict.Code = input.Code;
        dict.Remark = input.Remark;
        dict.UpdateTime = DateTime.Now;
        dict.UpdateUserId = 0;

        await _dictRepo.UpdateAsync(dict);
        return await Get(id);
    }

    [HttpDelete("api/dict/{id}")]
    [Permission(PermissionConsts.DictDelete)]
    public async Task Delete(long id)
    {
        var dict = await _dictRepo.GetByIdAsync(id);
        if (dict == null)
        {
            throw Oops.Oh("字典不存在");
        }

        await _dictItemRepo.DeleteAsync(di => di.DictId == id);
        await _dictRepo.DeleteAsync(id);
    }

    [HttpGet("api/dict/{code}/items")]
    [Permission(PermissionConsts.DictItemView)]
    public async Task<List<DictItemOutput>> GetDictItems(string code)
    {
        var dict = await _dictRepo.AsQueryable()
            .Where(d => d.Code == code)
            .FirstAsync();

        if (dict == null)
        {
            return new List<DictItemOutput>();
        }

        var items = await _dictItemRepo.AsQueryable()
            .Where(di => di.DictId == dict.Id && di.Status == 1)
            .OrderBy(di => di.Sort)
            .ToListAsync();

        return items.Select(di => new DictItemOutput
        {
            Id = di.Id,
            DictId = di.DictId,
            Label = di.Label,
            Value = di.Value,
            Sort = di.Sort,
            IsDefault = di.IsDefault,
            Status = di.Status
        }).ToList();
    }

    [HttpPost("api/dict-item")]
    [Permission(PermissionConsts.DictItemAdd)]
    public async Task<DictItemOutput> AddDictItem(AddDictItemInput input)
    {
        var dict = await _dictRepo.GetByIdAsync(input.DictId);
        if (dict == null)
        {
            throw Oops.Oh("字典不存在");
        }

        var item = new DictItem
        {
            DictId = input.DictId,
            Label = input.Label,
            Value = input.Value,
            Sort = input.Sort,
            IsDefault = input.IsDefault,
            Status = input.Status,
            TenantId = 0,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        item = await _dictItemRepo.InsertAsync(item);

        return new DictItemOutput
        {
            Id = item.Id,
            DictId = item.DictId,
            Label = item.Label,
            Value = item.Value,
            Sort = item.Sort,
            IsDefault = item.IsDefault,
            Status = item.Status
        };
    }

    [HttpPut("api/dict-item/{id}")]
    [Permission(PermissionConsts.DictItemEdit)]
    public async Task<DictItemOutput> UpdateDictItem(long id, UpdateDictItemInput input)
    {
        var item = await _dictItemRepo.GetByIdAsync(id);
        if (item == null)
        {
            throw Oops.Oh("字典项不存在");
        }

        item.DictId = input.DictId;
        item.Label = input.Label;
        item.Value = input.Value;
        item.Sort = input.Sort;
        item.IsDefault = input.IsDefault;
        item.Status = input.Status;
        item.UpdateTime = DateTime.Now;
        item.UpdateUserId = 0;

        await _dictItemRepo.UpdateAsync(item);

        return new DictItemOutput
        {
            Id = item.Id,
            DictId = item.DictId,
            Label = item.Label,
            Value = item.Value,
            Sort = item.Sort,
            IsDefault = item.IsDefault,
            Status = item.Status
        };
    }

    [HttpDelete("api/dict-item/{id}")]
    [Permission(PermissionConsts.DictItemDelete)]
    public async Task DeleteDictItem(long id)
    {
        var item = await _dictItemRepo.GetByIdAsync(id);
        if (item == null)
        {
            throw Oops.Oh("字典项不存在");
        }

        await _dictItemRepo.DeleteAsync(id);
    }

    private long GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
        return claim != null && long.TryParse(claim, out var id) ? id : 0;
    }
}