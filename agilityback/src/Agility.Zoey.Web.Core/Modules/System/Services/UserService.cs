using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using Agility.Zoey.Web.Core.Shared.Attributes;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 100)]
public class UserService : IDynamicApiController, ITransient
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<Dept> _deptRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        IRepository<User> userRepo,
        IRepository<UserRole> userRoleRepo,
        IRepository<Dept> deptRepo,
        IRepository<Role> roleRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _deptRepo = deptRepo;
        _roleRepo = roleRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("api/user/page")]
    [Permission(PermissionConsts.UserView)]
    public async Task<PageResult<UserOutput>> GetPage(UserPageInput input)
    {
        var tenantId = GetCurrentTenantId();
        var query = _userRepo.AsQueryable()
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        if (!string.IsNullOrEmpty(input.UserName))
        {
            query = query.Where(u => u.UserName.Contains(input.UserName));
        }

        if (!string.IsNullOrEmpty(input.Phone))
        {
            query = query.Where(u => u.Phone != null && u.Phone.Contains(input.Phone));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(u => u.Status == input.Status.Value);
        }

        if (input.DeptId.HasValue)
        {
            query = query.Where(u => u.DeptId == input.DeptId.Value);
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
            query = query.OrderBy(u => u.Id, OrderByType.Desc);
        }

        var totalCount = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var deptIds = users.Select(u => u.DeptId).Distinct().ToList();
        var depts = await _deptRepo.AsQueryable()
            .Where(d => deptIds.Contains(d.Id))
            .ToListAsync();
        var deptMap = depts.ToDictionary(d => d.Id, d => d.Name);

        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await _userRoleRepo.AsQueryable()
            .Where(ur => userIds.Contains(ur.UserId))
            .ToListAsync();
        var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
        var roles = await _roleRepo.AsQueryable()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
        var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);

        var userRoleMap = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => roleMap.GetValueOrDefault(ur.RoleId, string.Empty)).ToList());

        var roleIdMap = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => ur.RoleId).ToList());

        var items = users.Select(u => new UserOutput
        {
            Id = u.Id,
            UserName = u.UserName,
            NickName = u.NickName,
            Email = u.Email,
            Phone = u.Phone,
            Avatar = u.Avatar,
            DeptId = u.DeptId,
            DeptName = deptMap.GetValueOrDefault(u.DeptId, string.Empty),
            Status = u.Status,
            LastLoginTime = u.LastLoginTime,
            LastLoginIp = u.LastLoginIp,
            CreateTime = u.CreateTime,
            RoleNames = userRoleMap.GetValueOrDefault(u.Id, new List<string>()),
            RoleIds = roleIdMap.GetValueOrDefault(u.Id, new List<long>())
        }).ToList();

        return new PageResult<UserOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/user/{id}")]
    [Permission(PermissionConsts.UserView)]
    public async Task<UserOutput> Get(long id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw Oops.Oh("用户不存在");
        }

        var dept = await _deptRepo.GetByIdAsync(user.DeptId);
        var userRoles = await _userRoleRepo.AsQueryable()
            .Where(ur => ur.UserId == id)
            .ToListAsync();

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _roleRepo.AsQueryable()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();

        return new UserOutput
        {
            Id = user.Id,
            UserName = user.UserName,
            NickName = user.NickName,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            DeptId = user.DeptId,
            DeptName = dept?.Name ?? string.Empty,
            Status = user.Status,
            LastLoginTime = user.LastLoginTime,
            LastLoginIp = user.LastLoginIp,
            CreateTime = user.CreateTime,
            RoleNames = roles.Select(r => r.Name).ToList(),
            RoleIds = roleIds
        };
    }

    [HttpPost("api/user")]
    [Permission(PermissionConsts.UserAdd)]
    public async Task<UserOutput> Add(AddUserInput input)
    {
        var tenantId = GetCurrentTenantId();
        var exists = await _userRepo.AsQueryable()
            .AnyAsync(u => u.UserName == input.UserName && u.TenantId == tenantId && !u.IsDeleted);
        if (exists)
        {
            throw Oops.Oh("用户名已存在");
        }

        if (!string.IsNullOrEmpty(input.Email))
        {
            var emailExists = await _userRepo.AsQueryable()
                .AnyAsync(u => u.Email == input.Email && u.TenantId == tenantId && !u.IsDeleted);
            if (emailExists)
            {
                throw Oops.Oh("邮箱已存在");
            }
        }

        var currentUserId = GetCurrentUserId();
        var user = new User
        {
            UserName = input.UserName,
            Password = Md5Hash(input.Password),
            NickName = input.NickName,
            Email = input.Email,
            Phone = input.Phone,
            DeptId = input.DeptId,
            TenantId = tenantId,
            Status = input.Status,
            CreateTime = DateTime.Now,
            CreateUserId = currentUserId
        };

        try
        {
            await _userRepo.BeginTranAsync();

            user = await _userRepo.InsertAsync(user);

            if (input.RoleIds.Any())
            {
                var userRoles = input.RoleIds.Select(roleId => new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                }).ToList();
                await _userRepo.Context.Insertable(userRoles).ExecuteCommandAsync();
            }

            await _userRepo.CommitTranAsync();
        }
        catch
        {
            await _userRepo.RollbackTranAsync();
            throw;
        }

        return await Get(user.Id);
    }

    [HttpPut("api/user/{id}")]
    [Permission(PermissionConsts.UserEdit)]
    public async Task<UserOutput> Update(long id, UpdateUserInput input)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw Oops.Oh("用户不存在");
        }

        var tenantId = GetCurrentTenantId();
        var exists = await _userRepo.AsQueryable()
            .AnyAsync(u => u.UserName == user.UserName && u.Id != id && u.TenantId == tenantId && !u.IsDeleted);

        if (!string.IsNullOrEmpty(input.Email))
        {
            var emailExists = await _userRepo.AsQueryable()
                .AnyAsync(u => u.Email == input.Email && u.Id != id && u.TenantId == tenantId && !u.IsDeleted);
            if (emailExists)
            {
                throw Oops.Oh("邮箱已存在");
            }
        }

        user.NickName = input.NickName;
        user.Email = input.Email;
        user.Phone = input.Phone;
        user.DeptId = input.DeptId;
        user.Status = input.Status;
        user.UpdateTime = DateTime.Now;
        user.UpdateUserId = GetCurrentUserId();

        try
        {
            await _userRepo.BeginTranAsync();

            await _userRepo.UpdateAsync(user);

            await _userRepo.Context.Deleteable<UserRole>().Where(ur => ur.UserId == id).ExecuteCommandAsync();
            if (input.RoleIds.Any())
            {
                var userRoles = input.RoleIds.Select(roleId => new UserRole
                {
                    UserId = id,
                    RoleId = roleId
                }).ToList();
                await _userRepo.Context.Insertable(userRoles).ExecuteCommandAsync();
            }

            await _userRepo.CommitTranAsync();
        }
        catch
        {
            await _userRepo.RollbackTranAsync();
            throw;
        }

        return await Get(id);
    }

    [HttpDelete("api/user/{id}")]
    [Permission(PermissionConsts.UserDelete)]
    public async Task Delete(long id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw Oops.Oh("用户不存在");
        }

        if (user.UserName == "admin")
        {
            throw Oops.Oh("不能删除超级管理员");
        }

        try
        {
            await _userRepo.BeginTranAsync();

            await _userRepo.SoftDeleteAsync(id);
            await _userRepo.Context.Deleteable<UserRole>().Where(ur => ur.UserId == id).ExecuteCommandAsync();

            await _userRepo.CommitTranAsync();
        }
        catch
        {
            await _userRepo.RollbackTranAsync();
            throw;
        }
    }

    [HttpPut("api/user/{id}/status")]
    [Permission(PermissionConsts.UserSetStatus)]
    public async Task SetStatus(long id, int status)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw Oops.Oh("用户不存在");
        }

        if (user.UserName == "admin" && status != 1)
        {
            throw Oops.Oh("不能禁用超级管理员");
        }

        user.Status = status;
        user.UpdateTime = DateTime.Now;
        user.UpdateUserId = GetCurrentUserId();
        await _userRepo.UpdateAsync(user);
    }

    [HttpPut("api/user/{id}/reset-password")]
    [Permission(PermissionConsts.UserResetPassword)]
    public async Task ResetPassword(long id, string password)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw Oops.Oh("用户不存在");
        }

        user.Password = Md5Hash(password);
        user.UpdateTime = DateTime.Now;
        user.UpdateUserId = GetCurrentUserId();
        await _userRepo.UpdateAsync(user);
    }

    [HttpPost("api/user/export")]
    [Permission(PermissionConsts.UserExport)]
    public async Task<IActionResult> Export(UserPageInput input)
    {
        var pageResult = await GetPage(input);
        var bytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(pageResult.Items));
        return new FileContentResult(bytes, "application/json")
        {
            FileDownloadName = $"users_{DateTime.Now:yyyyMMddHHmmss}.json"
        };
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

    private static string Md5Hash(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}