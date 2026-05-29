using System.Reflection;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 120)]
public class MenuService : IDynamicApiController, ITransient
{
    private readonly IRepository<Menu> _menuRepo;

    public MenuService(IRepository<Menu> menuRepo)
    {
        _menuRepo = menuRepo;
    }

    [HttpGet("api/menu/tree")]
    [Permission(PermissionConsts.MenuView)]
    public async Task<List<MenuTreeOutput>> GetTree()
    {
        var menus = await _menuRepo.AsQueryable()
            .Where(m => !m.IsObsolete)
            .OrderBy(m => m.OrderNo)
            .ToListAsync();

        return BuildMenuTree(menus, 0);
    }

    [HttpGet("api/menu/routes")]
    [Permission(PermissionConsts.MenuView)]
    public async Task<List<MenuTreeOutput>> GetRoutes()
    {
        var menus = await _menuRepo.AsQueryable()
            .Where(m => m.Status == 1 && !m.IsObsolete && m.MenuType != 3)
            .OrderBy(m => m.OrderNo)
            .ToListAsync();

        return BuildMenuTree(menus, 0);
    }

    [HttpGet("api/menu/{id}")]
    [Permission(PermissionConsts.MenuView)]
    public async Task<MenuOutput> Get(long id)
    {
        var menu = await _menuRepo.GetByIdAsync(id);
        if (menu == null || menu.IsObsolete)
        {
            throw Oops.Oh("菜单不存在");
        }

        return new MenuOutput
        {
            Id = menu.Id,
            ParentId = menu.ParentId,
            MenuType = menu.MenuType,
            Name = menu.Name,
            Path = menu.Path,
            Component = menu.Component,
            Redirect = menu.Redirect,
            Icon = menu.Icon,
            Permission = menu.Permission,
            OrderNo = menu.OrderNo,
            IsHide = menu.IsHide,
            KeepAlive = menu.KeepAlive,
            Status = menu.Status,
            IsFrame = menu.IsFrame,
            FrameSrc = menu.FrameSrc,
            CreateTime = menu.CreateTime
        };
    }

    [HttpPost("api/menu")]
    [Permission(PermissionConsts.MenuAdd)]
    public async Task<MenuOutput> Add(AddMenuInput input)
    {
        var menu = new Menu
        {
            ParentId = input.ParentId,
            MenuType = input.MenuType,
            Name = input.Name,
            Path = input.Path,
            Component = input.Component,
            Redirect = input.Redirect,
            Icon = input.Icon,
            Permission = input.Permission,
            OrderNo = input.OrderNo,
            IsHide = input.IsHide,
            KeepAlive = input.KeepAlive,
            Status = input.Status,
            IsFrame = input.IsFrame,
            FrameSrc = input.FrameSrc,
            TenantId = 0,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        menu = await _menuRepo.InsertAsync(menu);
        return await Get(menu.Id);
    }

    [HttpPut("api/menu/{id}")]
    [Permission(PermissionConsts.MenuEdit)]
    public async Task<MenuOutput> Update(long id, UpdateMenuInput input)
    {
        var menu = await _menuRepo.GetByIdAsync(id);
        if (menu == null || menu.IsObsolete)
        {
            throw Oops.Oh("菜单不存在");
        }

        if (input.ParentId == id)
        {
            throw Oops.Oh("不能将菜单的父级设置为自身");
        }

        menu.ParentId = input.ParentId;
        menu.MenuType = input.MenuType;
        menu.Name = input.Name;
        menu.Path = input.Path;
        menu.Component = input.Component;
        menu.Redirect = input.Redirect;
        menu.Icon = input.Icon;
        menu.Permission = input.Permission;
        menu.OrderNo = input.OrderNo;
        menu.IsHide = input.IsHide;
        menu.KeepAlive = input.KeepAlive;
        menu.Status = input.Status;
        menu.IsFrame = input.IsFrame;
        menu.FrameSrc = input.FrameSrc;
        menu.UpdateTime = DateTime.Now;
        menu.UpdateUserId = 0;

        await _menuRepo.UpdateAsync(menu);
        return await Get(id);
    }

    [HttpDelete("api/menu/{id}")]
    [Permission(PermissionConsts.MenuDelete)]
    public async Task Delete(long id)
    {
        var menu = await _menuRepo.GetByIdAsync(id);
        if (menu == null || menu.IsObsolete)
        {
            throw Oops.Oh("菜单不存在");
        }

        var hasChildren = await _menuRepo.AsQueryable()
            .AnyAsync(m => m.ParentId == id && !m.IsObsolete);
        if (hasChildren)
        {
            throw Oops.Oh("存在子菜单，请先删除子菜单");
        }

        try
        {
            await _menuRepo.BeginTranAsync();

            menu.IsObsolete = true;
            menu.UpdateTime = DateTime.Now;
            await _menuRepo.UpdateAsync(menu);

            await _menuRepo.CommitTranAsync();
        }
        catch
        {
            await _menuRepo.RollbackTranAsync();
            throw;
        }
    }

    [HttpPut("api/menu/{id}/status")]
    [Permission(PermissionConsts.MenuSetStatus)]
    public async Task SetStatus(long id, int status)
    {
        var menu = await _menuRepo.GetByIdAsync(id);
        if (menu == null || menu.IsObsolete)
        {
            throw Oops.Oh("菜单不存在");
        }

        menu.Status = status;
        menu.UpdateTime = DateTime.Now;
        await _menuRepo.UpdateAsync(menu);
    }

    [HttpPost("api/menu/sync")]
    [Permission(PermissionConsts.MenuSync)]
    public async Task<int> SyncPermissions()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var controllerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IDynamicApiController).IsAssignableFrom(t))
            .ToList();

        var synced = 0;
        foreach (var controllerType in controllerTypes)
        {
            var apiSettings = controllerType.GetCustomAttribute<ApiDescriptionSettingsAttribute>();
            if (apiSettings == null) continue;

            var groupName = apiSettings.Group ?? string.Empty;
            if (string.IsNullOrEmpty(groupName)) continue;

            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var httpAttributes = method.GetCustomAttributes()
                    .Where(a => a.GetType().Name.StartsWith("Http"))
                    .ToList();

                if (!httpAttributes.Any()) continue;

                var methodLower = method.Name.ToLowerInvariant();
                var permissionCode = $"{groupName.ToLowerInvariant()}:{methodLower}";

                var existing = await _menuRepo.AsQueryable()
                    .Where(m => m.Permission == permissionCode && m.MenuType == 3)
                    .FirstAsync();

                if (existing == null)
                {
                    var buttonMenu = new Menu
                    {
                        ParentId = 0,
                        MenuType = 3,
                        Name = method.Name,
                        Permission = permissionCode,
                        TenantId = 0,
                        Status = 1,
                        CreateTime = DateTime.Now,
                        CreateUserId = 0
                    };
                    await _menuRepo.InsertAsync(buttonMenu);
                    synced++;
                }
            }
        }

        return synced;
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