using System.Security.Cryptography;
using System.Text;
using Agility.Zoey.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Agility.Zoey.Data.SeedData;

public static class SeedDataInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<SqlSugarScope>();

        var tenant = await SeedTenantAsync(client);
        var adminUser = await SeedAdminUserAsync(client, tenant.Id);
        var superAdminRole = await SeedSuperAdminRoleAsync(client, tenant.Id);
        var normalRole = await SeedNormalRoleAsync(client, tenant.Id);
        await SeedUserRoleAsync(client, adminUser.Id, superAdminRole.Id);
        await SeedMenusAsync(client, tenant.Id);
        await SeedSystemSettingsAsync(client, tenant.Id);
        await EnsureArticleTableAsync(client);
    }

    private static async Task<Tenant> SeedTenantAsync(SqlSugarScope client)
    {
        var existing = await client.Queryable<Tenant>().FirstAsync(t => t.Code == "default");
        if (existing != null)
        {
            return existing;
        }

        var tenant = new Tenant
        {
            Name = "默认租户",
            Code = "default",
            TenantType = 1,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        await client.Insertable(tenant).ExecuteReturnEntityAsync();
        return tenant;
    }

    private static async Task<User> SeedAdminUserAsync(SqlSugarScope client, long tenantId)
    {
        var existing = await client.Queryable<User>().FirstAsync(u => u.UserName == "admin");
        if (existing != null)
        {
            return existing;
        }

        var password = Md5Hash("Admin@123");

        var user = new User
        {
            UserName = "admin",
            Password = password,
            NickName = "系统管理员",
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        await client.Insertable(user).ExecuteReturnEntityAsync();
        return user;
    }

    private static async Task<Role> SeedSuperAdminRoleAsync(SqlSugarScope client, long tenantId)
    {
        var existing = await client.Queryable<Role>().FirstAsync(r => r.Code == "super_admin");
        if (existing != null)
        {
            return existing;
        }

        var role = new Role
        {
            Name = "超级管理员",
            Code = "super_admin",
            TenantId = tenantId,
            DataScope = 1,
            Sort = 0,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        await client.Insertable(role).ExecuteReturnEntityAsync();
        return role;
    }

    private static async Task<Role> SeedNormalRoleAsync(SqlSugarScope client, long tenantId)
    {
        var existing = await client.Queryable<Role>().FirstAsync(r => r.Code == "normal_user");
        if (existing != null)
        {
            return existing;
        }

        var role = new Role
        {
            Name = "普通用户",
            Code = "normal_user",
            TenantId = tenantId,
            DataScope = 4,
            Sort = 1,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        await client.Insertable(role).ExecuteReturnEntityAsync();
        return role;
    }

    private static async Task SeedUserRoleAsync(SqlSugarScope client, long userId, long roleId)
    {
        var existing = await client.Queryable<UserRole>().FirstAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (existing != null)
        {
            return;
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        await client.Insertable(userRole).ExecuteCommandAsync();
    }

    private static async Task SeedMenusAsync(SqlSugarScope client, long tenantId)
    {
        var existing = await client.Queryable<Menu>().AnyAsync(m => m.TenantId == tenantId);
        if (existing)
        {
            return;
        }

        var menus = new List<Menu>();

        var dashboard = new Menu
        {
            Name = "仪表盘",
            Path = "/dashboard",
            Component = "dashboard/index",
            Icon = "dashboard",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 1,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(dashboard);

        var systemMgr = new Menu
        {
            Name = "系统管理",
            Path = "/system",
            Component = "LAYOUT",
            Icon = "setting",
            MenuType = 1,
            ParentId = 0,
            OrderNo = 2,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(systemMgr);

        var userMgr = new Menu
        {
            Name = "用户管理",
            Path = "user",
            Component = "system/user/index",
            Permission = "system:user:list",
            Icon = "user",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 1,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(userMgr);

        var roleMgr = new Menu
        {
            Name = "角色管理",
            Path = "role",
            Component = "system/role/index",
            Permission = "system:role:list",
            Icon = "team",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 2,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(roleMgr);

        var menuMgr = new Menu
        {
            Name = "菜单管理",
            Path = "menu",
            Component = "system/menu/index",
            Permission = "system:menu:list",
            Icon = "menu",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 3,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(menuMgr);

        var deptMgr = new Menu
        {
            Name = "部门管理",
            Path = "dept",
            Component = "system/dept/index",
            Permission = "system:dept:list",
            Icon = "apartment",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 4,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(deptMgr);

        var tenantMgr = new Menu
        {
            Name = "租户管理",
            Path = "tenant",
            Component = "system/tenant/index",
            Permission = "system:tenant:list",
            Icon = "bank",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 5,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(tenantMgr);

        var dictMgr = new Menu
        {
            Name = "字典管理",
            Path = "dict",
            Component = "system/dict/index",
            Permission = "system:dict:list",
            Icon = "book",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 6,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(dictMgr);

        var fileMgr = new Menu
        {
            Name = "文件管理",
            Path = "file",
            Component = "system/file/index",
            Permission = "system:file:list",
            Icon = "file",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 7,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(fileMgr);

        var sysSetting = new Menu
        {
            Name = "系统设置",
            Path = "setting",
            Component = "system/setting/index",
            Permission = "system:setting:list",
            Icon = "tool",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 8,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(sysSetting);

        var opLog = new Menu
        {
            Name = "操作日志",
            Path = "operationLog",
            Component = "system/operationLog/index",
            Permission = "system:operationLog:list",
            Icon = "file-text",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 9,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(opLog);

        var loginLog = new Menu
        {
            Name = "登录日志",
            Path = "loginLog",
            Component = "system/loginLog/index",
            Permission = "system:loginLog:list",
            Icon = "login",
            MenuType = 2,
            ParentId = 0,
            OrderNo = 10,
            TenantId = tenantId,
            Status = 1,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };
        menus.Add(loginLog);

        var result = await client.Insertable(menus).ExecuteReturnEntityAsync();

        userMgr.ParentId = systemMgr.Id;
        roleMgr.ParentId = systemMgr.Id;
        menuMgr.ParentId = systemMgr.Id;
        deptMgr.ParentId = systemMgr.Id;
        tenantMgr.ParentId = systemMgr.Id;
        dictMgr.ParentId = systemMgr.Id;
        fileMgr.ParentId = systemMgr.Id;
        sysSetting.ParentId = systemMgr.Id;
        opLog.ParentId = systemMgr.Id;
        loginLog.ParentId = systemMgr.Id;

        await client.Updateable(new List<Menu> { userMgr, roleMgr, menuMgr, deptMgr, tenantMgr, dictMgr, fileMgr, sysSetting, opLog, loginLog })
            .UpdateColumns(it => new { it.ParentId })
            .ExecuteCommandAsync();
    }

    private static async Task SeedSystemSettingsAsync(SqlSugarScope client, long tenantId)
    {
        var existing = await client.Queryable<SystemSetting>().AnyAsync(s => s.TenantId == tenantId && s.ConfigKey == "FileStorage.Provider");
        if (existing)
        {
            return;
        }

        var settings = new List<SystemSetting>
        {
            new()
            {
                TenantId = tenantId,
                ConfigKey = "FileStorage.Provider",
                ConfigValue = "Local",
                ValueType = "String",
                Group = "FileStorage",
                Description = "文件存储方式",
                IsCacheable = true,
                Sort = 1,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "FileStorage.LocalPath",
                ConfigValue = "uploads",
                ValueType = "String",
                Group = "FileStorage",
                Description = "本地存储路径",
                IsCacheable = true,
                Sort = 2,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "FileStorage.MaxSize",
                ConfigValue = "10485760",
                ValueType = "Int",
                Group = "FileStorage",
                Description = "最大上传文件大小(字节)",
                IsCacheable = true,
                Sort = 3,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "Security.PasswordMinLength",
                ConfigValue = "6",
                ValueType = "Int",
                Group = "Security",
                Description = "密码最小长度",
                IsCacheable = true,
                Sort = 1,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "Security.PasswordExpireDays",
                ConfigValue = "90",
                ValueType = "Int",
                Group = "Security",
                Description = "密码过期天数",
                IsCacheable = true,
                Sort = 2,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "Security.LoginFailMaxCount",
                ConfigValue = "5",
                ValueType = "Int",
                Group = "Security",
                Description = "最大登录失败次数",
                IsCacheable = true,
                Sort = 3,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "Security.LockDurationMinutes",
                ConfigValue = "30",
                ValueType = "Int",
                Group = "Security",
                Description = "账户锁定时间(分钟)",
                IsCacheable = true,
                Sort = 4,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "Log.EnableOperationLog",
                ConfigValue = "true",
                ValueType = "Bool",
                Group = "Log",
                Description = "启用操作日志",
                IsCacheable = true,
                Sort = 1,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "Log.EnableLoginLog",
                ConfigValue = "true",
                ValueType = "Bool",
                Group = "Log",
                Description = "启用登录日志",
                IsCacheable = true,
                Sort = 2,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "System.Theme",
                ConfigValue = "default",
                ValueType = "String",
                Group = "System",
                Description = "系统主题",
                IsCacheable = true,
                Sort = 1,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            },
            new()
            {
                TenantId = tenantId,
                ConfigKey = "System.Title",
                ConfigValue = "Agility管理平台",
                ValueType = "String",
                Group = "System",
                Description = "系统标题",
                IsCacheable = true,
                Sort = 2,
                Status = 1,
                CreateTime = DateTime.Now,
                CreateUserId = 0
            }
        };

        await client.Insertable(settings).ExecuteCommandAsync();
    }

    private static async Task EnsureArticleTableAsync(SqlSugarScope client)
    {
        client.CodeFirst.InitTables(typeof(Article));
        await Task.CompletedTask;
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