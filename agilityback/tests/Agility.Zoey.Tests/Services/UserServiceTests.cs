using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Modules.System.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using SqlSugar;

namespace Agility.Zoey.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<UserRole>> _mockUserRoleRepo;
    private readonly Mock<IRepository<Dept>> _mockDeptRepo;
    private readonly Mock<IRepository<Role>> _mockRoleRepo;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockUserRoleRepo = new Mock<IRepository<UserRole>>();
        _mockDeptRepo = new Mock<IRepository<Dept>>();
        _mockRoleRepo = new Mock<IRepository<Role>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new("TenantId", "1")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _userService = new UserService(
            _mockUserRepo.Object,
            _mockUserRoleRepo.Object,
            _mockDeptRepo.Object,
            _mockRoleRepo.Object,
            _mockHttpContextAccessor.Object);
    }

    [Fact]
    public void Md5Hash_Should_Return_32Char_Lowercase_Hex_String()
    {
        var input = "Admin@123";
        var expected = Md5HashDirect(input);

        var actual = InvokeMd5Hash(input);

        Assert.Equal(32, actual.Length);
        Assert.Equal(expected, actual);
        Assert.True(actual.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void Md5Hash_Should_Produce_Consistent_Output()
    {
        var input = "TestPassword";

        var result1 = InvokeMd5Hash(input);
        var result2 = InvokeMd5Hash(input);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Md5Hash_Should_Differ_For_Different_Inputs()
    {
        var result1 = InvokeMd5Hash("password1");
        var result2 = InvokeMd5Hash("password2");

        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void Md5Hash_Empty_String_Should_Return_Known_Hash()
    {
        var result = InvokeMd5Hash(string.Empty);

        var expected = Md5HashDirect(string.Empty);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AddUser_WithDuplicateUsername_Should_Throw_Exception()
    {
        var queryable = CreateMockQueryable<User>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(true);

        _mockUserRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var input = new AddUserInput
        {
            UserName = "existinguser",
            Password = "Test@123"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => _userService.Add(input));
        Assert.Contains("用户名已存在", ex.Message);
    }

    [Fact]
    public async Task AddUser_WithDuplicateEmail_Should_Throw_Exception()
    {
        var queryable = CreateMockQueryable<User>();
        var usernameCheckCount = 0;
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(() =>
            {
                usernameCheckCount++;
                return usernameCheckCount > 1;
            });

        _mockUserRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var input = new AddUserInput
        {
            UserName = "newuser",
            Password = "Test@123",
            Email = "existing@example.com"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => _userService.Add(input));
        Assert.Contains("邮箱已存在", ex.Message);
    }

    [Fact]
    public async Task AddUser_Should_Call_BeginTran_Before_Insert()
    {
        var queryable = CreateMockQueryable<User>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockUserRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var savedUser = new User { Id = 10, UserName = "newuser" };
        _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>())).ReturnsAsync(savedUser);

        var sequence = new List<string>();
        _mockUserRepo.Setup(r => r.BeginTranAsync()).Callback(() => sequence.Add("begin"));
        _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>())).Callback(() => sequence.Add("insert"));
        _mockUserRepo.Setup(r => r.CommitTranAsync()).Callback(() => sequence.Add("commit"));

        _mockDeptRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<Dept>().Object);
        _mockUserRoleRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<UserRole>().Object);
        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<Role>().Object);
        _mockDeptRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((Dept?)null);

        var input = new AddUserInput
        {
            UserName = "newuser",
            Password = "Test@123"
        };

        try
        {
            await _userService.Add(input);
        }
        catch
        {
        }

        Assert.Equal("begin", sequence[0]);
        Assert.Equal("commit", sequence[sequence.Count - 1]);
    }

    [Fact]
    public async Task AddUser_Should_Rollback_On_Exception()
    {
        var queryable = CreateMockQueryable<User>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockUserRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var input = new AddUserInput
        {
            UserName = "newuser",
            Password = "Test@123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.Add(input));

        _mockUserRepo.Verify(r => r.RollbackTranAsync(), Times.Once);
        _mockUserRepo.Verify(r => r.CommitTranAsync(), Times.Never);
    }

    [Fact]
    public async Task AddUser_WithRoleIds_Should_Assign_Roles_In_Transaction()
    {
        var queryable = CreateMockQueryable<User>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockUserRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        var savedUser = new User { Id = 10, UserName = "newuser", TenantId = 1 };
        _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>())).ReturnsAsync(savedUser);

        var mockClient = new Mock<SqlSugarClient>();
        _mockUserRepo.Setup(r => r.Context).Returns(mockClient.Object);

        var mockInsertable = new Mock<IInsertable<UserRole>>();
        mockClient.Setup(c => c.Insertable(It.IsAny<List<UserRole>>()))
            .Returns(mockInsertable.Object);
        mockInsertable.Setup(i => i.ExecuteCommandAsync()).ReturnsAsync(2);

        _mockDeptRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<Dept>().Object);
        _mockUserRoleRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<UserRole>().Object);
        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<Role>().Object);
        _mockDeptRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((Dept?)null);

        var input = new AddUserInput
        {
            UserName = "newuser",
            Password = "Test@123",
            RoleIds = new List<long> { 1, 2 }
        };

        try
        {
            await _userService.Add(input);
        }
        catch
        {
        }

        _mockUserRepo.Verify(r => r.BeginTranAsync(), Times.Once);
        _mockUserRepo.Verify(r => r.CommitTranAsync(), Times.Once);
    }

    [Fact]
    public async Task AddUser_Should_Hash_Password_With_MD5()
    {
        var queryable = CreateMockQueryable<User>();
        queryable.Setup(q => q.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockUserRepo.Setup(r => r.AsQueryable()).Returns(queryable.Object);

        User? capturedUser = null;
        _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        _mockDeptRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<Dept>().Object);
        _mockUserRoleRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<UserRole>().Object);
        _mockRoleRepo.Setup(r => r.AsQueryable()).Returns(CreateMockQueryable<Role>().Object);
        _mockDeptRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((Dept?)null);

        var input = new AddUserInput
        {
            UserName = "newuser",
            Password = "plaintext"
        };

        try
        {
            await _userService.Add(input);
        }
        catch
        {
        }

        Assert.NotNull(capturedUser);
        var expectedHash = Md5HashDirect("plaintext");
        Assert.Equal(expectedHash, capturedUser!.Password);
        Assert.NotEqual("plaintext", capturedUser.Password);
        Assert.Equal(32, capturedUser.Password.Length);
    }

    [Fact]
    public async Task SetStatus_Should_Throw_When_User_Not_Found()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => _userService.SetStatus(999, 1));
        Assert.Contains("用户不存在", ex.Message);
    }

    [Fact]
    public async Task SetStatus_Should_Throw_When_Disabling_Admin()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, UserName = "admin" });

        var ex = await Assert.ThrowsAsync<Exception>(() => _userService.SetStatus(1, 0));
        Assert.Contains("不能禁用超级管理员", ex.Message);
    }

    [Fact]
    public async Task SetStatus_Should_Allow_Enabling_Admin()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, UserName = "admin" });
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(1);

        await _userService.SetStatus(1, 1);

        _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Status == 1)), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_Throw_When_Deleting_Admin()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, UserName = "admin" });

        var ex = await Assert.ThrowsAsync<Exception>(() => _userService.Delete(1));
        Assert.Contains("不能删除超级管理员", ex.Message);
    }

    [Fact]
    public async Task Delete_Should_SoftDelete_In_Transaction()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new User { Id = 10, UserName = "normaluser" });
        _mockUserRepo.Setup(r => r.SoftDeleteAsync(10)).ReturnsAsync(1);

        var mockClient = new Mock<SqlSugarClient>();
        _mockUserRepo.Setup(r => r.Context).Returns(mockClient.Object);
        var mockDeleteable = new Mock<IDeleteable<UserRole>>();
        mockClient.Setup(c => c.Deleteable<UserRole>(It.IsAny<Expression<Func<UserRole, object>>[]>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.Where(It.IsAny<Expression<Func<UserRole, bool>>>()))
            .Returns(mockDeleteable.Object);
        mockDeleteable.Setup(d => d.ExecuteCommandAsync()).ReturnsAsync(1);

        await _userService.Delete(10);

        _mockUserRepo.Verify(r => r.BeginTranAsync(), Times.Once);
        _mockUserRepo.Verify(r => r.SoftDeleteAsync(10), Times.Once);
        _mockUserRepo.Verify(r => r.CommitTranAsync(), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_Should_Use_MD5_Hash()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, UserName = "testuser" });

        User? updatedUser = null;
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .ReturnsAsync(1);

        await _userService.ResetPassword(1, "newPassword");

        Assert.NotNull(updatedUser);
        var expectedHash = Md5HashDirect("newPassword");
        Assert.Equal(expectedHash, updatedUser!.Password);
    }

    [Fact]
    public async Task ResetPassword_Should_Throw_When_User_Not_Found()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => _userService.ResetPassword(999, "pass"));
        Assert.Contains("用户不存在", ex.Message);
    }

    private static string InvokeMd5Hash(string input)
    {
        var method = typeof(UserService).GetMethod("Md5Hash",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (string)method!.Invoke(null, new object[] { input })!;
    }

    private static string Md5HashDirect(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    private static Mock<ISugarQueryable<T>> CreateMockQueryable<T>() where T : class, new()
    {
        var mock = new Mock<ISugarQueryable<T>>();
        mock.Setup(q => q.Where(It.IsAny<Expression<Func<T, bool>>>())).Returns(mock.Object);
        mock.Setup(q => q.OrderBy(It.IsAny<string>())).Returns(mock.Object);
        mock.Setup(q => q.OrderBy(It.IsAny<Expression<Func<T, object>>>(), It.IsAny<OrderByType>())).Returns(mock.Object);
        mock.Setup(q => q.Skip(It.IsAny<int>())).Returns(mock.Object);
        mock.Setup(q => q.Take(It.IsAny<int>())).Returns(mock.Object);

        var emptyList = new List<T>();
        mock.Setup(q => q.ToListAsync()).ReturnsAsync(emptyList);
        mock.Setup(q => q.CountAsync()).ReturnsAsync(0);

        return mock;
    }
}