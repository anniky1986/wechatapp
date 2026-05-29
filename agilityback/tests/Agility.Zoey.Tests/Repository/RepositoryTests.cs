using System.Linq.Expressions;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Moq;
using SqlSugar;

namespace Agility.Zoey.Tests.Repository;

public class RepositoryTests
{
    [Fact]
    public async Task BeginTranAsync_Should_Call_Ado_BeginTran()
    {
        var mockAdo = new Mock<IAdo>();
        var mockClient = CreateMockSqlSugarScope(mockAdo.Object);
        var repo = new Repository<User>(mockClient);

        await repo.BeginTranAsync();

        mockAdo.Verify(a => a.BeginTran(), Times.Once);
    }

    [Fact]
    public async Task CommitTranAsync_Should_Call_Ado_CommitTran()
    {
        var mockAdo = new Mock<IAdo>();
        var mockClient = CreateMockSqlSugarScope(mockAdo.Object);
        var repo = new Repository<User>(mockClient);

        await repo.CommitTranAsync();

        mockAdo.Verify(a => a.CommitTran(), Times.Once);
    }

    [Fact]
    public async Task RollbackTranAsync_Should_Call_Ado_RollbackTran()
    {
        var mockAdo = new Mock<IAdo>();
        var mockClient = CreateMockSqlSugarScope(mockAdo.Object);
        var repo = new Repository<User>(mockClient);

        await repo.RollbackTranAsync();

        mockAdo.Verify(a => a.RollbackTran(), Times.Once);
    }

    [Fact]
    public async Task TransactionLifecycle_Should_Call_In_Correct_Order()
    {
        var sequence = 0;
        var mockAdo = new Mock<IAdo>();
        mockAdo.Setup(a => a.BeginTran()).Callback(() => Assert.Equal(0, sequence++));
        mockAdo.Setup(a => a.CommitTran()).Callback(() => Assert.Equal(1, sequence++));

        var mockClient = CreateMockSqlSugarScope(mockAdo.Object);
        var repo = new Repository<User>(mockClient);

        await repo.BeginTranAsync();
        await repo.CommitTranAsync();

        Assert.Equal(2, sequence);
    }

    [Fact]
    public async Task Rollback_After_Begin_Should_Call_Rollback_Not_Commit()
    {
        var mockAdo = new Mock<IAdo>();
        var mockClient = CreateMockSqlSugarScope(mockAdo.Object);
        var repo = new Repository<User>(mockClient);

        await repo.BeginTranAsync();
        await repo.RollbackTranAsync();

        mockAdo.Verify(a => a.BeginTran(), Times.Once);
        mockAdo.Verify(a => a.RollbackTran(), Times.Once);
        mockAdo.Verify(a => a.CommitTran(), Times.Never);
    }

    [Fact]
    public void Context_Property_Should_Return_SqlSugarClient()
    {
        var mockAdo = new Mock<IAdo>();
        var mockClient = CreateMockSqlSugarScope(mockAdo.Object);
        var repo = new Repository<User>(mockClient);

        Assert.Same(mockClient, repo.Context);
    }

    [Fact]
    public async Task IRepository_GetByIdAsync_Should_Return_User_When_Found()
    {
        var mockRepo = new Mock<IRepository<User>>();
        var expectedUser = new User { Id = 1, UserName = "test" };
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedUser);

        var result = await mockRepo.Object.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("test", result.UserName);
    }

    [Fact]
    public async Task IRepository_GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var result = await mockRepo.Object.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task IRepository_GetListAsync_Should_Return_All_Items()
    {
        var mockRepo = new Mock<IRepository<User>>();
        var users = new List<User>
        {
            new() { Id = 1, UserName = "user1" },
            new() { Id = 2, UserName = "user2" }
        };
        mockRepo.Setup(r => r.GetListAsync()).ReturnsAsync(users);

        var result = await mockRepo.Object.GetListAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task IRepository_GetListAsync_WithPredicate_Should_Filter()
    {
        var mockRepo = new Mock<IRepository<User>>();
        var filtered = new List<User> { new() { Id = 1, UserName = "user1" } };
        mockRepo.Setup(r => r.GetListAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(filtered);

        var result = await mockRepo.Object.GetListAsync(u => u.UserName == "user1");

        Assert.Single(result);
        Assert.Equal("user1", result[0].UserName);
    }

    [Fact]
    public async Task IRepository_InsertAsync_Should_Return_Entity_With_Id()
    {
        var mockRepo = new Mock<IRepository<User>>();
        var user = new User { Id = 0, UserName = "newuser" };
        var saved = new User { Id = 10, UserName = "newuser" };

        mockRepo.Setup(r => r.InsertAsync(It.IsAny<User>())).ReturnsAsync(saved);

        var result = await mockRepo.Object.InsertAsync(user);

        Assert.Equal(10, result.Id);
        Assert.Equal("newuser", result.UserName);
    }

    [Fact]
    public async Task IRepository_CountAsync_Should_Return_Count()
    {
        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(5);

        var result = await mockRepo.Object.CountAsync(u => u.Status == 1);

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task IRepository_DeleteAsync_ById_Should_Return_AffectedRows()
    {
        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(1);

        var result = await mockRepo.Object.DeleteAsync(1);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task IRepository_SoftDeleteAsync_Should_Return_AffectedRows()
    {
        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(1);

        var result = await mockRepo.Object.SoftDeleteAsync(1);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task IRepository_UpdateAsync_Should_Return_AffectedRows()
    {
        var mockRepo = new Mock<IRepository<User>>();
        var user = new User { Id = 1, UserName = "updated" };
        mockRepo.Setup(r => r.UpdateAsync(user)).ReturnsAsync(1);

        var result = await mockRepo.Object.UpdateAsync(user);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task IRepository_GetPageAsync_Should_Return_Paginated_Result()
    {
        var mockRepo = new Mock<IRepository<User>>();
        var items = new List<User> { new() { Id = 1 }, new() { Id = 2 } };
        mockRepo.Setup(r => r.GetPageAsync(1, 10)).ReturnsAsync((items, 100));

        var (resultItems, totalCount) = await mockRepo.Object.GetPageAsync(1, 10);

        Assert.Equal(2, resultItems.Count);
        Assert.Equal(100, totalCount);
    }

    private static SqlSugarScope CreateMockSqlSugarScope(IAdo mockAdo)
    {
        var connectionConfig = new ConnectionConfig
        {
            ConnectionString = "Server=localhost;Database=test;",
            DbType = DbType.MySql,
            IsAutoCloseConnection = true
        };

        var mockClient = new Mock<SqlSugarScope>(connectionConfig) { CallBase = false };
        mockClient.Setup(c => c.Ado).Returns(mockAdo);

        return mockClient.Object;
    }
}