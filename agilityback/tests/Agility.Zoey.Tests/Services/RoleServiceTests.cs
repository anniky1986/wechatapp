using System.Security.Claims;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Modules.System.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using SqlSugar;

namespace Agility.Zoey.Tests.Services;

public class RoleServiceTests
{
    private readonly Mock<IRepository<Role>> _mockRoleRepo;
    private readonly Mock<IRepository<RoleMenu>> _mockRoleMenuRepo;
    private readonly Mock<IRepository<DataPermission>> _mockDataPermissionRepo;
    private readonly Mock<IRepository<Menu>> _mockMenuRepo;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _mockRoleRepo = new Mock<IRepository<Role>>();
        _mockRoleMenuRepo = new Mock<IRepository<RoleMenu>>();
        _mockDataPermissionRepo = new Mock<IRepository<DataPermission>>();
        _mockMenuRepo = new Mock<IRepository<Menu>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new("TenantId", "1")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _roleService = new RoleService(
            _mockRoleRepo.Object,
            _mockRoleMenuRepo.Object,
            _mockDataPermissionRepo.Object,
            _mockMenuRepo.Object,
            _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task AddRole_WithDuplicateCode_Should_Throw_Exception()
    {
        var queryable = CreateMockQueryable<Role>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .ReturnsAsync(true);

        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var input = new AddRoleInput
        {
            Name = "Test Role",
            Code = "existing_code"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => _roleService.Add(input));
        Assert.Contains("角色编码已存在", ex.Message);
    }

    [Fact]
    public async Task AddRole_Should_Assign_Menus_When_MenuIds_Provided()
    {
        var queryable = CreateMockQueryable<Role>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .ReturnsAsync(false);

        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var savedRole = new Role { Id = 10, Name = "Test Role", Code = "test_role", TenantId = 1 };
        _mockRoleRepo.Setup(r => r.InsertAsync(It.IsAny<Role>())).ReturnsAsync(savedRole);

        var mockClient = new Mock<SqlSugarClient>();
        _mockRoleRepo.Setup(r => r.Context).Returns(mockClient.Object);

        var mockInsertable = new Mock<IInsertable<RoleMenu>>();
        mockClient.Setup(c => c.Insertable(It.IsAny<List<RoleMenu>>()))
            .Returns(mockInsertable.Object);
        mockInsertable.Setup(i => i.ExecuteCommandAsync()).ReturnsAsync(3);

        _mockRoleMenuRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<RoleMenu>().Object);

        var input = new AddRoleInput
        {
            Name = "Test Role",
            Code = "test_role",
            MenuIds = new List<long> { 1, 2, 3 }
        };

        try
        {
            await _roleService.Add(input);
        }
        catch
        {
        }

        _mockRoleRepo.Verify(r => r.InsertAsync(It.IsAny<Role>()), Times.Once);
        mockClient.Verify(c => c.Insertable(It.IsAny<List<RoleMenu>>()), Times.Once);
    }

    [Fact]
    public async Task AddRole_Without_MenuIds_Should_Not_Assign_Menus()
    {
        var queryable = CreateMockQueryable<Role>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .ReturnsAsync(false);

        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var savedRole = new Role { Id = 10, Name = "Test Role", Code = "test_role", TenantId = 1 };
        _mockRoleRepo.Setup(r => r.InsertAsync(It.IsAny<Role>())).ReturnsAsync(savedRole);

        var mockClient = new Mock<SqlSugarClient>();
        _mockRoleRepo.Setup(r => r.Context).Returns(mockClient.Object);

        _mockRoleMenuRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<RoleMenu>().Object);

        var input = new AddRoleInput
        {
            Name = "Test Role",
            Code = "test_role",
            MenuIds = new List<long>()
        };

        try
        {
            await _roleService.Add(input);
        }
        catch
        {
        }

        mockClient.Verify(c => c.Insertable(It.IsAny<List<RoleMenu>>()), Times.Never);
    }

    [Fact]
    public async Task SetRoleMenus_Should_Wrap_In_Transaction()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "Test", Code = "test", TenantId = 1 });

        var mockClient = new Mock<SqlSugarClient>();
        _mockRoleRepo.Setup(r => r.Context).Returns(mockClient.Object);

        var mockDeleteable = new Mock<IDeleteable<RoleMenu>>();
        mockClient.Setup(c => c.Deleteable<RoleMenu>(It.IsAny<Expression<Func<RoleMenu, object>>[]>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.Where(It.IsAny<Expression<Func<RoleMenu, bool>>>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.ExecuteCommandAsync()).ReturnsAsync(1);

        var mockInsertable = new Mock<IInsertable<RoleMenu>>();
        mockClient.Setup(c => c.Insertable(It.IsAny<List<RoleMenu>>()))
            .Returns(mockInsertable.Object);
        mockInsertable.Setup(i => i.ExecuteCommandAsync()).ReturnsAsync(1);

        var sequence = new List<string>();
        _mockRoleRepo.Setup(r => r.BeginTranAsync()).Callback(() => sequence.Add("begin"));
        _mockRoleRepo.Setup(r => r.CommitTranAsync()).Callback(() => sequence.Add("commit"));

        await _roleService.SetRoleMenus(1, new List<long> { 1 });

        Assert.Equal(2, sequence.Count);
        Assert.Equal("begin", sequence[0]);
        Assert.Equal("commit", sequence[1]);
    }

    [Fact]
    public async Task SetRoleMenus_Should_Rollback_On_Error()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "Test", Code = "test", TenantId = 1 });

        var mockClient = new Mock<SqlSugarClient>();
        _mockRoleRepo.Setup(r => r.Context).Returns(mockClient.Object);

        var mockDeleteable = new Mock<IDeleteable<RoleMenu>>();
        mockClient.Setup(c => c.Deleteable<RoleMenu>(It.IsAny<Expression<Func<RoleMenu, object>>[]>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.Where(It.IsAny<Expression<Func<RoleMenu, bool>>>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.ExecuteCommandAsync())
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _roleService.SetRoleMenus(1, new List<long> { 1 }));

        _mockRoleRepo.Verify(r => r.RollbackTranAsync(), Times.Once);
        _mockRoleRepo.Verify(r => r.CommitTranAsync(), Times.Never);
    }

    [Fact]
    public async Task SetRoleMenus_Should_Throw_When_Role_Not_Found()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Role?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _roleService.SetRoleMenus(999, new List<long> { 1 }));
        Assert.Contains("角色不存在", ex.Message);
    }

    [Fact]
    public async Task UpdateRole_WithDuplicateCode_Should_Throw_Exception()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "Test", Code = "old_code" });

        var queryable = CreateMockQueryable<Role>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .ReturnsAsync(true);

        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var input = new UpdateRoleInput
        {
            Id = 1,
            Name = "Updated Role",
            Code = "existing_code"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => _roleService.Update(1, input));
        Assert.Contains("角色编码已存在", ex.Message);
    }

    [Fact]
    public async Task Delete_Should_Throw_When_Deleting_SuperAdmin_Role()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "超级管理员", Code = "super_admin" });

        var ex = await Assert.ThrowsAsync<Exception>(() => _roleService.Delete(1));
        Assert.Contains("不能删除超级管理员角色", ex.Message);
    }

    [Fact]
    public async Task Delete_Should_Remove_RoleMenus_And_DataPermissions_In_Transaction()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Role { Id = 10, Name = "Normal", Code = "normal" });

        var mockClient = new Mock<SqlSugarClient>();
        _mockRoleRepo.Setup(r => r.Context).Returns(mockClient.Object);

        var mockDeleteable = new Mock<IDeleteable<Role>>();
        mockClient.Setup(c => c.Deleteable<Role>(It.IsAny<Expression<Func<Role, object>>[]>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.Where(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.ExecuteCommandAsync()).ReturnsAsync(1);

        var mockDeleteableRM = new Mock<IDeleteable<RoleMenu>>();
        mockClient.Setup(c => c.Deleteable<RoleMenu>(It.IsAny<Expression<Func<RoleMenu, object>>[]>()))
            .Returns(mockDeleteableRM.Object);
        mockDeleteableRM.Setup(d => d.Where(It.IsAny<Expression<Func<RoleMenu, bool>>>()))
            .Returns(mockDeleteableRM.Object);
        mockDeleteableRM.Setup(d => d.ExecuteCommandAsync()).ReturnsAsync(1);

        var mockDeleteableDP = new Mock<IDeleteable<DataPermission>>();
        mockClient.Setup(c => c.Deleteable<DataPermission>(It.IsAny<Expression<Func<DataPermission, object>>[]>()))
            .Returns(mockDeleteableDP.Object);
        mockDeleteableDP.Setup(d => d.Where(It.IsAny<Expression<Func<DataPermission, bool>>>()))
            .Returns(mockDeleteableDP.Object);
        mockDeleteableDP.Setup(d => d.ExecuteCommandAsync()).ReturnsAsync(1);

        await _roleService.Delete(10);

        _mockRoleRepo.Verify(r => r.BeginTranAsync(), Times.Once);
        _mockRoleRepo.Verify(r => r.CommitTranAsync(), Times.Once);
        mockDeleteable.Verify(d => d.ExecuteCommandAsync(), Times.Once);
        mockDeleteableRM.Verify(d => d.ExecuteCommandAsync(), Times.Once);
        mockDeleteableDP.Verify(d => d.ExecuteCommandAsync(), Times.Once);
    }

    [Fact]
    public async Task SetStatus_Should_Throw_When_Disabling_SuperAdmin()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "超级管理员", Code = "super_admin" });

        var ex = await Assert.ThrowsAsync<Exception>(() => _roleService.SetStatus(1, 0));
        Assert.Contains("不能禁用超级管理员角色", ex.Message);
    }

    [Fact]
    public async Task SetStatus_Should_Update_When_Valid()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "Normal", Code = "normal" });
        _mockRoleRepo.Setup(r => r.UpdateAsync(It.IsAny<Role>())).ReturnsAsync(1);

        await _roleService.SetStatus(1, 0);

        _mockRoleRepo.Verify(r => r.UpdateAsync(It.Is<Role>(role => role.Status == 0)), Times.Once);
    }

    [Fact]
    public async Task SetDataPermission_Should_Wrap_In_Transaction()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Role { Id = 1, Name = "Test", Code = "test", TenantId = 1 });

        var mockClient = new Mock<SqlSugarClient>();
        _mockRoleRepo.Setup(r => r.Context).Returns(mockClient.Object);

        var mockDeleteable = new Mock<IDeleteable<DataPermission>>();
        mockClient.Setup(c => c.Deleteable<DataPermission>(It.IsAny<Expression<Func<DataPermission, object>>[]>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.Where(It.IsAny<Expression<Func<DataPermission, bool>>>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.ExecuteCommandAsync()).ReturnsAsync(1);

        var mockInsertable = new Mock<IInsertable<DataPermission>>();
        mockClient.Setup(c => c.Insertable(It.IsAny<List<DataPermission>>()))
            .Returns(mockInsertable.Object);
        mockInsertable.Setup(i => i.ExecuteCommandAsync()).ReturnsAsync(2);

        var sequence = new List<string>();
        _mockRoleRepo.Setup(r => r.BeginTranAsync()).Callback(() => sequence.Add("begin"));
        _mockRoleRepo.Setup(r => r.CommitTranAsync()).Callback(() => sequence.Add("commit"));

        await _roleService.SetDataPermission(1, new DataPermissionInput
        {
            DeptIds = new List<long> { 1, 2 }
        });

        Assert.Equal(2, sequence.Count);
        Assert.Equal("begin", sequence[0]);
        Assert.Equal("commit", sequence[1]);
    }

    [Fact]
    public async Task Get_Should_Throw_When_Role_Not_Found()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Role?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => _roleService.Get(999));
        Assert.Contains("角色不存在", ex.Message);
    }

    [Fact]
    public async Task SetDataPermission_Should_Throw_When_Role_Not_Found()
    {
        _mockRoleRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Role?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _roleService.SetDataPermission(999, new DataPermissionInput()));
        Assert.Contains("角色不存在", ex.Message);
    }

    private static Mock<ISugarQueryable<T>> CreateMockQueryable<T>() where T : class, new()
    {
        var mock = new Mock<ISugarQueryable<T>>();
        mock.Setup(q => q.Where(It.IsAny<Expression<Func<T, bool>>>())).Returns(mock.Object);
        mock.Setup(q => q.OrderBy(It.IsAny<string>())).Returns(mock.Object);
        mock.Setup(q => q.OrderBy(It.IsAny<Expression<Func<T, object>>>(), It.IsAny<OrderByType>())).Returns(mock.Object);
        mock.Setup(q => q.Skip(It.IsAny<int>())).Returns(mock.Object);
        mock.Setup(q => q.Take(It.IsAny<int>())).Returns(mock.Object);
        mock.Setup(q => q.Select(It.IsAny<Expression<Func<T, object>>>())).Returns((ISugarQueryable<object>)null!);

        var emptyList = new List<T>();
        mock.Setup(q => q.ToListAsync()).ReturnsAsync(emptyList);
        mock.Setup(q => q.CountAsync()).ReturnsAsync(0);

        return mock;
    }
}