using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using SqlSugar;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 140)]
public class TenantService : IDynamicApiController, ITransient
{
    private readonly IRepository<Tenant> _tenantRepo;

    public TenantService(IRepository<Tenant> tenantRepo)
    {
        _tenantRepo = tenantRepo;
    }

    [HttpGet("api/tenant/page")]
    public async Task<PageResult<TenantOutput>> GetPage(TenantPageInput input)
    {
        var query = _tenantRepo.AsQueryable();

        if (!string.IsNullOrEmpty(input.Name))
        {
            query = query.Where(t => t.Name.Contains(input.Name));
        }

        if (!string.IsNullOrEmpty(input.Code))
        {
            query = query.Where(t => t.Code.Contains(input.Code));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(t => t.Status == input.Status.Value);
        }

        var page = input.Page < 1 ? 1 : input.Page;
        var pageSize = input.PageSize < 1 ? SystemConsts.DefaultPageSize : input.PageSize;
        if (pageSize > SystemConsts.MaxPageSize) pageSize = SystemConsts.MaxPageSize;

        if (!string.IsNullOrEmpty(input.SortField) && !string.IsNullOrEmpty(input.SortOrder))
        {
            var isAsc = input.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);
            query = query.OrderBy($"{input.SortField} {(isAsc ? "ASC" : "DESC")}");
        }
        else
        {
            query = query.OrderBy(t => t.Id, OrderByType.Desc);
        }

        var totalCount = await query.CountAsync();
        var tenants = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = tenants.Select(t => new TenantOutput
        {
            Id = t.Id,
            Name = t.Name,
            Code = t.Code,
            TenantType = t.TenantType,
            DbConnectionString = t.DbConnectionString,
            Status = t.Status,
            ExpireTime = t.ExpireTime,
            CreateTime = t.CreateTime
        }).ToList();

        return new PageResult<TenantOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/tenant/{id}")]
    public async Task<TenantOutput> Get(long id)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id);
        if (tenant == null)
        {
            throw Oops.Oh("租户不存在");
        }

        return new TenantOutput
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Code = tenant.Code,
            TenantType = tenant.TenantType,
            DbConnectionString = tenant.DbConnectionString,
            Status = tenant.Status,
            ExpireTime = tenant.ExpireTime,
            CreateTime = tenant.CreateTime
        };
    }

    [HttpPost("api/tenant")]
    public async Task<TenantOutput> Add(AddTenantInput input)
    {
        var exists = await _tenantRepo.AsQueryable()
            .AnyAsync(t => t.Code == input.Code);
        if (exists)
        {
            throw Oops.Oh("租户编码已存在");
        }

        var tenant = new Tenant
        {
            Name = input.Name,
            Code = input.Code,
            TenantType = input.TenantType,
            DbConnectionString = input.DbConnectionString,
            Status = input.Status,
            ExpireTime = input.ExpireTime,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        tenant = await _tenantRepo.InsertAsync(tenant);
        return await Get(tenant.Id);
    }

    [HttpPut("api/tenant/{id}")]
    public async Task<TenantOutput> Update(long id, UpdateTenantInput input)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id);
        if (tenant == null)
        {
            throw Oops.Oh("租户不存在");
        }

        var exists = await _tenantRepo.AsQueryable()
            .AnyAsync(t => t.Code == input.Code && t.Id != id);
        if (exists)
        {
            throw Oops.Oh("租户编码已存在");
        }

        tenant.Name = input.Name;
        tenant.Code = input.Code;
        tenant.TenantType = input.TenantType;
        tenant.DbConnectionString = input.DbConnectionString;
        tenant.Status = input.Status;
        tenant.ExpireTime = input.ExpireTime;
        tenant.UpdateTime = DateTime.Now;
        tenant.UpdateUserId = 0;

        await _tenantRepo.UpdateAsync(tenant);
        return await Get(id);
    }

    [HttpDelete("api/tenant/{id}")]
    public async Task Delete(long id)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id);
        if (tenant == null)
        {
            throw Oops.Oh("租户不存在");
        }

        if (tenant.Code == SystemConsts.DefaultTenantCode)
        {
            throw Oops.Oh("不能删除默认租户");
        }

        await _tenantRepo.DeleteAsync(id);
    }

    [HttpPut("api/tenant/{id}/status")]
    public async Task SetStatus(long id, int status)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id);
        if (tenant == null)
        {
            throw Oops.Oh("租户不存在");
        }

        if (tenant.Code == SystemConsts.DefaultTenantCode && status != 1)
        {
            throw Oops.Oh("不能禁用默认租户");
        }

        tenant.Status = status;
        tenant.UpdateTime = DateTime.Now;
        await _tenantRepo.UpdateAsync(tenant);
    }
}