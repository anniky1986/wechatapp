using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 110)]
public class RoleService : IDynamicApiController, ITransient
{
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<RoleMenu> _roleMenuRepo;
    private readonly IRepository<DataPermission> _dataPermissionRepo;
    private readonly IRepository<Menu> _menuRepo;

    public RoleService(
        IRepository<Role> roleRepo,
        IRepository<RoleMenu> roleMenuRepo,
        IRepository<DataPermission> dataPermissionRepo,
        IRepository<Menu> menuRepo)
    {
        _roleRepo = roleRepo;
        _roleMenuRepo = roleMenuRepo;
        _dataPermissionRepo = dataPermissionRepo;
        _menuRepo = menuRepo;
    }

    [HttpGet("api/role/page")]
    [Permission(PermissionConsts.RoleView)]
    public async Task<PageResult<RoleOutput>> GetPage(RolePageInput input)
    {
        var query = _roleRepo.AsQueryable();

        if (!string.IsNullOrEmpty(input.Name))
        {
            query = query.Where(r => r.Name.Contains(input.Name));
        }

        if (!string.IsNullOrEmpty(input.Code))
        {
            query = query.Where(r => r.Code.Contains(input.Code));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(r => r.Status == input.Status.Value);
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
            query = query.OrderBy(r => r.Sort);
        }

        var totalCount = await query.CountAsync();
        var roles = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var roleIds = roles.Select(r => r.Id).ToList();
        var roleMenus = await _roleMenuRepo.AsQueryable()
            .Where(rm => roleIds.Contains(rm.RoleId))
            .ToListAsync();
        var roleMenuMap = roleMenus
            .GroupBy(rm => rm.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(rm => rm.MenuId).ToList());

        var items = roles.Select(r => new RoleOutput
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Sort = r.Sort,
            Status = r.Status,
            DataScope = r.DataScope,
            Remark = r.Remark,
            CreateTime = r.CreateTime,
            MenuIds = roleMenuMap.GetValueOrDefault(r.Id, new List<long>())
        }).ToList();

        return new PageResult<RoleOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/role/list")]
    [Permission(PermissionConsts.RoleView)]
    public async Task<List<RoleOutput>> GetList()
    {
        var roles = await _roleRepo.AsQueryable()
            .Where(r => r.Status == 1)
            .OrderBy(r => r.Sort)
            .ToListAsync();

        return roles.Select(r => new RoleOutput
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Sort = r.Sort,
            Status = r.Status,
            DataScope = r.DataScope,
            Remark = r.Remark,
            CreateTime = r.CreateTime
        }).ToList();
    }

    [HttpGet("api/role/{id}")]
    [Permission(PermissionConsts.RoleView)]
    public async Task<RoleOutput> Get(long id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
        {
            throw Oops.Oh("角色不存在");
        }

        var menuIds = await _roleMenuRepo.AsQueryable()
            .Where(rm => rm.RoleId == id)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        return new RoleOutput
        {
            Id = role.Id,
            Name = role.Name,
            Code = role.Code,
            Sort = role.Sort,
            Status = role.Status,
            DataScope = role.DataScope,
            Remark = role.Remark,
            CreateTime = role.CreateTime,
            MenuIds = menuIds
        };
    }

    [HttpPost("api/role")]
    [Permission(PermissionConsts.RoleAdd)]
    public async Task<RoleOutput> Add(AddRoleInput input)
    {
        var exists = await _roleRepo.AsQueryable()
            .AnyAsync(r => r.Code == input.Code);
        if (exists)
        {
            throw Oops.Oh("角色编码已存在");
        }

        var role = new Role
        {
            Name = input.Name,
            Code = input.Code,
            Sort = input.Sort,
            Status = input.Status,
            DataScope = input.DataScope,
            Remark = input.Remark,
            TenantId = 0,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        role = await _roleRepo.InsertAsync(role);

        if (input.MenuIds.Any())
        {
            await AssignRoleMenus(role.Id, input.MenuIds);
        }

        return await Get(role.Id);
    }

    [HttpPut("api/role/{id}")]
    [Permission(PermissionConsts.RoleEdit)]
    public async Task<RoleOutput> Update(long id, UpdateRoleInput input)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
        {
            throw Oops.Oh("角色不存在");
        }

        var exists = await _roleRepo.AsQueryable()
            .AnyAsync(r => r.Code == input.Code && r.Id != id);
        if (exists)
        {
            throw Oops.Oh("角色编码已存在");
        }

        role.Name = input.Name;
        role.Code = input.Code;
        role.Sort = input.Sort;
        role.Status = input.Status;
        role.DataScope = input.DataScope;
        role.Remark = input.Remark;
        role.UpdateTime = DateTime.Now;
        role.UpdateUserId = 0;

        await _roleRepo.UpdateAsync(role);

        await _roleRepo.Context.Deleteable<RoleMenu>().Where(rm => rm.RoleId == id).ExecuteCommandAsync();
        if (input.MenuIds.Any())
        {
            await AssignRoleMenus(id, input.MenuIds);
        }

        return await Get(id);
    }

    [HttpDelete("api/role/{id}")]
    [Permission(PermissionConsts.RoleDelete)]
    public async Task Delete(long id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
        {
            throw Oops.Oh("角色不存在");
        }

        if (role.Code == SystemConsts.SuperAdminRoleCode)
        {
            throw Oops.Oh("不能删除超级管理员角色");
        }

        await _roleRepo.Context.Deleteable<Role>().Where(r => r.Id == id).ExecuteCommandAsync();
        await _roleRepo.Context.Deleteable<RoleMenu>().Where(rm => rm.RoleId == id).ExecuteCommandAsync();
        await _roleRepo.Context.Deleteable<DataPermission>().Where(dp => dp.RoleId == id).ExecuteCommandAsync();
    }

    [HttpPut("api/role/{id}/status")]
    [Permission(PermissionConsts.RoleSetStatus)]
    public async Task SetStatus(long id, int status)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
        {
            throw Oops.Oh("角色不存在");
        }

        if (role.Code == SystemConsts.SuperAdminRoleCode && status != 1)
        {
            throw Oops.Oh("不能禁用超级管理员角色");
        }

        role.Status = status;
        role.UpdateTime = DateTime.Now;
        await _roleRepo.UpdateAsync(role);
    }

    [HttpPut("api/role/{id}/menu")]
    [Permission(PermissionConsts.RoleSetMenu)]
    public async Task SetRoleMenus(long id, List<long> menuIds)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
        {
            throw Oops.Oh("角色不存在");
        }

        await _roleRepo.Context.Deleteable<RoleMenu>().Where(rm => rm.RoleId == id).ExecuteCommandAsync();
        if (menuIds.Any())
        {
            await AssignRoleMenus(id, menuIds);
        }
    }

    [HttpPut("api/role/{id}/data-permission")]
    [Permission(PermissionConsts.RoleSetDataPermission)]
    public async Task SetDataPermission(long id, DataPermissionInput input)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
        {
            throw Oops.Oh("角色不存在");
        }

        await _roleRepo.Context.Deleteable<DataPermission>().Where(dp => dp.RoleId == id).ExecuteCommandAsync();
        if (input.DeptIds.Any())
        {
            var permissions = input.DeptIds.Select(deptId => new DataPermission
            {
                RoleId = id,
                DeptId = deptId,
                TenantId = role.TenantId,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            }).ToList();
            await _roleRepo.Context.Insertable(permissions).ExecuteCommandAsync();
        }
    }

    private async Task AssignRoleMenus(long roleId, List<long> menuIds)
    {
        var roleMenus = menuIds.Select(menuId => new RoleMenu
        {
            RoleId = roleId,
            MenuId = menuId
        }).ToList();
        await _roleRepo.Context.Insertable(roleMenus).ExecuteCommandAsync();
    }
}