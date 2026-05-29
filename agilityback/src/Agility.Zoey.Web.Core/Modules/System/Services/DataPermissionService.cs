using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 155)]
public class DataPermissionService : IDynamicApiController, ITransient
{
    private readonly IRepository<DataPermission> _dataPermissionRepo;
    private readonly IRepository<Dept> _deptRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<UserRole> _userRoleRepo;

    public DataPermissionService(
        IRepository<DataPermission> dataPermissionRepo,
        IRepository<Dept> deptRepo,
        IRepository<Role> roleRepo,
        IRepository<UserRole> userRoleRepo)
    {
        _dataPermissionRepo = dataPermissionRepo;
        _deptRepo = deptRepo;
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
    }

    [HttpGet("api/data-permission/role/{roleId}")]
    [Permission(PermissionConsts.RoleSetDataPermission)]
    public async Task<List<DataPermissionOutput>> GetRoleDataPermissions(long roleId)
    {
        var permissions = await _dataPermissionRepo.AsQueryable()
            .Where(dp => dp.RoleId == roleId)
            .ToListAsync();

        var deptIds = permissions.Select(dp => dp.DeptId).Distinct().ToList();
        var depts = await _deptRepo.AsQueryable()
            .Where(d => deptIds.Contains(d.Id))
            .ToListAsync();

        var role = await _roleRepo.GetByIdAsync(roleId);

        return permissions.Select(dp =>
        {
            var dept = depts.FirstOrDefault(d => d.Id == dp.DeptId);
            return new DataPermissionOutput
            {
                Id = dp.Id,
                RoleId = dp.RoleId,
                RoleName = role?.Name ?? string.Empty,
                DeptId = dp.DeptId,
                DeptName = dept?.Name ?? string.Empty,
                CreateTime = dp.CreateTime
            };
        }).ToList();
    }

    [HttpGet("api/data-permission/user/{userId}")]
    [Permission(PermissionConsts.RoleSetDataPermission)]
    public async Task<List<DataPermissionOutput>> GetUserDataPermissions(long userId)
    {
        var userRoles = await _userRoleRepo.AsQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (userRoles.Count == 0)
        {
            return new List<DataPermissionOutput>();
        }

        var permissions = await _dataPermissionRepo.AsQueryable()
            .Where(dp => userRoles.Contains(dp.RoleId))
            .ToListAsync();

        var deptIds = permissions.Select(dp => dp.DeptId).Distinct().ToList();
        var depts = await _deptRepo.AsQueryable()
            .Where(d => deptIds.Contains(d.Id))
            .ToListAsync();

        var roles = await _roleRepo.AsQueryable()
            .Where(r => userRoles.Contains(r.Id))
            .ToListAsync();

        return permissions.Select(dp =>
        {
            var dept = depts.FirstOrDefault(d => d.Id == dp.DeptId);
            var role = roles.FirstOrDefault(r => r.Id == dp.RoleId);
            return new DataPermissionOutput
            {
                Id = dp.Id,
                RoleId = dp.RoleId,
                RoleName = role?.Name ?? string.Empty,
                DeptId = dp.DeptId,
                DeptName = dept?.Name ?? string.Empty,
                CreateTime = dp.CreateTime
            };
        }).ToList();
    }
}

public record DataPermissionOutput
{
    public long Id { get; set; }
    public long RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public long DeptId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}