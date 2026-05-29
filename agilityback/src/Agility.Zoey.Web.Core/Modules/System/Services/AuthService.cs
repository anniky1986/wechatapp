using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Core.Enums;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Furion;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 90)]
public class AuthService : IDynamicApiController, ITransient
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<RoleMenu> _roleMenuRepo;
    private readonly IRepository<Menu> _menuRepo;
    private readonly IRepository<Dept> _deptRepo;
    private readonly IRepository<LoginLog> _loginLogRepo;
    private readonly IRepository<SystemSetting> _settingRepo;
    private readonly IRepository<DictItem> _dictItemRepo;
    private readonly IRepository<Dict> _dictRepo;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<UserRole> userRoleRepo,
        IRepository<RoleMenu> roleMenuRepo,
        IRepository<Menu> menuRepo,
        IRepository<Dept> deptRepo,
        IRepository<LoginLog> loginLogRepo,
        IRepository<SystemSetting> settingRepo,
        IRepository<DictItem> dictItemRepo,
        IRepository<Dict> dictRepo,
        IMemoryCache cache,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
        _roleMenuRepo = roleMenuRepo;
        _menuRepo = menuRepo;
        _deptRepo = deptRepo;
        _loginLogRepo = loginLogRepo;
        _settingRepo = settingRepo;
        _dictItemRepo = dictItemRepo;
        _dictRepo = dictRepo;
        _cache = cache;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("api/auth/login")]
    public async Task<LoginOutput> Login(LoginInput input)
    {
        var user = await _userRepo.AsQueryable()
            .Where(u => u.UserName == input.UserName && !u.IsDeleted)
            .FirstAsync();

        if (user == null)
        {
            await RecordLoginLog(null, input.UserName, 0, "用户不存在", LoginType.Password);
            throw Oops.Oh("用户名或密码错误");
        }

        if (user.Status != 1)
        {
            await RecordLoginLog(user.Id, user.UserName, 0, "账户已禁用", LoginType.Password);
            throw Oops.Oh("账户已被禁用，请联系管理员");
        }

        if (user.LockEndTime.HasValue && user.LockEndTime > DateTime.Now)
        {
            await RecordLoginLog(user.Id, user.UserName, 0, "账户已锁定", LoginType.Password);
            throw Oops.Oh($"账户已被锁定，请于 {user.LockEndTime:yyyy-MM-dd HH:mm:ss} 后重试");
        }

        var passwordHash = Md5Hash(input.Password);
        if (user.Password != passwordHash)
        {
            user.LoginFailCount++;
            var maxFailCount = _configuration.GetValue<int>("Security:LoginFailMaxCount", 5);
            var lockMinutes = _configuration.GetValue<int>("Security:LockDurationMinutes", 30);
            if (user.LoginFailCount >= maxFailCount)
            {
                user.LockEndTime = DateTime.Now.AddMinutes(lockMinutes);
            }
            await _userRepo.UpdateAsync(user);
            await RecordLoginLog(user.Id, user.UserName, 0, "密码错误", LoginType.Password);
            throw Oops.Oh("用户名或密码错误");
        }

        user.LoginFailCount = 0;
        user.LockEndTime = null;
        user.LastLoginTime = DateTime.Now;
        user.LastLoginIp = GetClientIp();
        await _userRepo.UpdateAsync(user);

        var roles = await GetUserRoles(user.Id);
        var menus = await GetUserMenus(user.Id, user.TenantId);
        var permissions = await GetUserPermissions(user.Id, user.TenantId);

        var userInfo = await BuildUserInfo(user, roles);

        var token = GenerateToken(user, roles.Select(r => r.Code).ToList(), permissions);
        var refreshToken = await GenerateRefreshToken(user.Id);

        await RecordLoginLog(user.Id, user.UserName, 1, "登录成功", LoginType.Password);

        return new LoginOutput
        {
            Token = token,
            RefreshToken = refreshToken,
            UserInfo = userInfo,
            Menus = menus,
            Permissions = permissions
        };
    }

    [HttpPost("api/auth/init")]
    public async Task<InitOutput> Init()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw Oops.Oh("未登录或登录已过期");
        }

        var tenantIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
        long tenantId = 0;
        if (!string.IsNullOrEmpty(tenantIdClaim))
        {
            long.TryParse(tenantIdClaim, out tenantId);
        }

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
        {
            throw Oops.Oh("用户不存在");
        }

        var roles = await GetUserRoles(user.Id);
        var menus = await GetUserMenus(user.Id, tenantId);
        var permissions = await GetUserPermissions(user.Id, tenantId);
        var userInfo = await BuildUserInfo(user, roles);
        var systemConfig = await GetSystemConfig(tenantId);
        var dictData = await GetDictData(tenantId);

        return new InitOutput
        {
            UserInfo = userInfo,
            Menus = menus,
            Permissions = permissions,
            SystemConfig = systemConfig,
            DictData = dictData
        };
    }

    [HttpPost("api/auth/refresh-token")]
    public async Task<LoginOutput> RefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw Oops.Oh("刷新令牌不能为空");
        }

        var cacheKey = $"auth:refresh:{refreshToken}";
        if (!_cache.TryGetValue(cacheKey, out long userId))
        {
            throw Oops.Oh("刷新令牌无效或已过期");
        }

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null || user.Status != 1)
        {
            throw Oops.Oh("用户不存在或已禁用");
        }

        _cache.Remove(cacheKey);

        var roles = await GetUserRoles(user.Id);
        var menus = await GetUserMenus(user.Id, user.TenantId);
        var permissions = await GetUserPermissions(user.Id, user.TenantId);
        var userInfo = await BuildUserInfo(user, roles);

        var token = GenerateToken(user, roles.Select(r => r.Code).ToList(), permissions);
        var newRefreshToken = await GenerateRefreshToken(user.Id);

        return new LoginOutput
        {
            Token = token,
            RefreshToken = newRefreshToken,
            UserInfo = userInfo,
            Menus = menus,
            Permissions = permissions
        };
    }

    [HttpPost("api/auth/logout")]
    public async Task Logout()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var blacklist = _cache.GetOrCreate<HashSet<string>>("auth:blacklist", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromHours(24);
                    return new HashSet<string>();
                });
                blacklist?.Add(token);
            }
        }
    }

    [HttpPost("api/auth/change-password")]
    public async Task ChangePassword(ChangePasswordInput input)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw Oops.Oh("未登录或登录已过期");
        }

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
        {
            throw Oops.Oh("用户不存在");
        }

        var oldPasswordHash = Md5Hash(input.OldPassword);
        if (user.Password != oldPasswordHash)
        {
            throw Oops.Oh("原密码错误");
        }

        user.Password = Md5Hash(input.NewPassword);
        await _userRepo.UpdateAsync(user);
    }

    private string GenerateToken(User user, List<string> roles, List<string> permissions)
    {
        return JWTEncryption.Encrypt(new Dictionary<string, object>
        {
            { ClaimTypes.NameIdentifier, user.Id.ToString() },
            { ClaimTypes.Name, user.UserName },
            { "TenantId", user.TenantId.ToString() },
            { "NickName", user.NickName ?? string.Empty },
            { "DeptId", user.DeptId.ToString() },
            { ClaimTypes.Role, roles },
            { "Permission", permissions }
        });
    }

    private async Task<string> GenerateRefreshToken(long userId)
    {
        var refreshToken = Guid.NewGuid().ToString("N");
        var cacheKey = $"auth:refresh:{refreshToken}";
        _cache.Set(cacheKey, userId, TimeSpan.FromDays(7));
        return refreshToken;
    }

    private async Task<List<Role>> GetUserRoles(long userId)
    {
        var roleIds = await _userRoleRepo.AsQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return new List<Role>();
        }

        return await _roleRepo.AsQueryable()
            .Where(r => roleIds.Contains(r.Id) && r.Status == 1)
            .ToListAsync();
    }

    private async Task<List<MenuTreeOutput>> GetUserMenus(long userId, long tenantId)
    {
        var roleIds = await _userRoleRepo.AsQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return new List<MenuTreeOutput>();
        }

        var menuIds = await _roleMenuRepo.AsQueryable()
            .Where(rm => roleIds.Contains(rm.RoleId))
            .Select(rm => rm.MenuId)
            .Distinct()
            .ToListAsync();

        if (!menuIds.Any())
        {
            return new List<MenuTreeOutput>();
        }

        var menus = await _menuRepo.AsQueryable()
            .Where(m => menuIds.Contains(m.Id) && m.Status == 1 && !m.IsObsolete)
            .OrderBy(m => m.OrderNo)
            .ToListAsync();

        return BuildMenuTree(menus, 0);
    }

    private async Task<List<string>> GetUserPermissions(long userId, long tenantId)
    {
        var roleIds = await _userRoleRepo.AsQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return new List<string>();
        }

        var menuIds = await _roleMenuRepo.AsQueryable()
            .Where(rm => roleIds.Contains(rm.RoleId))
            .Select(rm => rm.MenuId)
            .Distinct()
            .ToListAsync();

        if (!menuIds.Any())
        {
            return new List<string>();
        }

        return await _menuRepo.AsQueryable()
            .Where(m => menuIds.Contains(m.Id) && m.Status == 1 && !string.IsNullOrEmpty(m.Permission) && !m.IsObsolete)
            .Select(m => m.Permission!)
            .Distinct()
            .ToListAsync();
    }

    private async Task<UserInfo> BuildUserInfo(User user, List<Role> roles)
    {
        var deptName = string.Empty;
        if (user.DeptId > 0)
        {
            var dept = await _deptRepo.GetByIdAsync(user.DeptId);
            deptName = dept?.Name ?? string.Empty;
        }

        return new UserInfo
        {
            Id = user.Id,
            UserName = user.UserName,
            NickName = user.NickName,
            Avatar = user.Avatar,
            DeptId = user.DeptId,
            DeptName = deptName,
            Roles = roles.Select(r => r.Code).ToList(),
            Permissions = new List<string>()
        };
    }

    private async Task<Dictionary<string, string>> GetSystemConfig(long tenantId)
    {
        var settings = _cache.GetOrCreate("system:settings", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return _settingRepo.AsQueryable()
                .Where(s => s.Status == 1)
                .ToListAsync()
                .GetAwaiter()
                .GetResult();
        });

        if (settings == null)
        {
            return new Dictionary<string, string>();
        }

        return settings
            .Where(s => s.ConfigValue != null)
            .ToDictionary(s => s.ConfigKey, s => s.ConfigValue!);
    }

    private async Task<Dictionary<string, List<DictItemOutput>>> GetDictData(long tenantId)
    {
        var dicts = await _dictRepo.AsQueryable().ToListAsync();
        var result = new Dictionary<string, List<DictItemOutput>>();

        foreach (var dict in dicts)
        {
            var items = _cache.GetOrCreate($"dict:items:{dict.Code}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return _dictItemRepo.AsQueryable()
                    .Where(di => di.DictId == dict.Id && di.Status == 1)
                    .OrderBy(di => di.Sort)
                    .ToListAsync()
                    .GetAwaiter()
                    .GetResult();
            });

            if (items != null)
            {
                result[dict.Code] = items.Select(di => new DictItemOutput
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
        }

        return result;
    }

    private async Task RecordLoginLog(long? userId, string userName, int status, string message, LoginType loginType)
    {
        var log = new LoginLog
        {
            UserId = userId ?? 0,
            UserName = userName,
            LoginType = (int)loginType,
            Status = status,
            Message = message,
            IpAddress = GetClientIp(),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty,
            LoginTime = DateTime.Now,
            CreateTime = DateTime.Now,
            CreateUserId = userId ?? 0
        };

        await _loginLogRepo.InsertAsync(log);
    }

    private string GetClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "unknown";

        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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

    private static List<MenuTreeOutput> BuildMenuTree(List<Menu> menus, long parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.OrderNo)
            .Select(m => new MenuTreeOutput
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                Path = m.Path,
                Component = m.Component,
                Redirect = m.Redirect,
                Icon = m.Icon,
                Permission = m.Permission,
                MenuType = m.MenuType,
                OrderNo = m.OrderNo,
                IsHide = m.IsHide,
                KeepAlive = m.KeepAlive,
                Status = m.Status,
                IsFrame = m.IsFrame,
                FrameSrc = m.FrameSrc,
                Children = BuildMenuTree(menus, m.Id)
            })
            .ToList();
    }
}