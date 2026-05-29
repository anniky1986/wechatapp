using System.Linq.Expressions;
using System.Reflection;
using Agility.Zoey.Core.Entities;
using SqlSugar;

namespace Agility.Zoey.Tests;

public class DataIntegrityTests
{
    [Fact]
    public void User_Should_Have_Unique_Index_On_UserName_And_TenantId()
    {
        var indexes = typeof(User).GetCustomAttributes<SugarIndex>().ToList();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_username");

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
        Assert.Contains("TenantId", uniqueIndex.IndexFields.Keys);
        Assert.Contains("UserName", uniqueIndex.IndexFields.Keys);
    }

    [Fact]
    public void User_Should_Have_Unique_Index_On_Email_And_TenantId()
    {
        var indexes = typeof(User).GetCustomAttributes<SugarIndex>().ToList();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_email");

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
        Assert.Contains("TenantId", uniqueIndex.IndexFields.Keys);
        Assert.Contains("Email", uniqueIndex.IndexFields.Keys);
    }

    [Fact]
    public void Role_Should_Have_Unique_Index_On_Code_And_TenantId()
    {
        var indexes = typeof(Role).GetCustomAttributes<SugarIndex>().ToList();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_code");

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
        Assert.Contains("TenantId", uniqueIndex.IndexFields.Keys);
        Assert.Contains("Code", uniqueIndex.IndexFields.Keys);
    }

    [Fact]
    public void Tenant_Should_Have_Single_Unique_Index_On_Code()
    {
        var indexes = typeof(Tenant).GetCustomAttributes<SugarIndex>().ToList();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_code");

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
        Assert.Contains("Code", uniqueIndex.IndexFields.Keys);
    }

    [Fact]
    public void Tenant_Should_Have_Only_One_Unique_Index()
    {
        var indexes = typeof(Tenant).GetCustomAttributes<SugarIndex>()
            .Count(i => i.IsUnique);

        Assert.Equal(1, indexes);
    }

    [Fact]
    public void Dict_Should_Have_Unique_Index_On_Code_And_TenantId()
    {
        var indexes = typeof(Dict).GetCustomAttributes<SugarIndex>().ToList();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_code");

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
        Assert.Contains("TenantId", uniqueIndex.IndexFields.Keys);
        Assert.Contains("Code", uniqueIndex.IndexFields.Keys);
    }

    [Fact]
    public void SystemSetting_Should_Have_Unique_Index_On_ConfigKey_And_TenantId()
    {
        var indexes = typeof(SystemSetting).GetCustomAttributes<SugarIndex>().ToList();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_configkey");

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
        Assert.Contains("TenantId", uniqueIndex.IndexFields.Keys);
        Assert.Contains("ConfigKey", uniqueIndex.IndexFields.Keys);
    }

    [Fact]
    public void All_Unique_Indexes_Should_Be_Unique()
    {
        var entityTypes = new[]
        {
            typeof(User), typeof(Role), typeof(Tenant),
            typeof(Dict), typeof(SystemSetting)
        };

        foreach (var type in entityTypes)
        {
            var indexes = type.GetCustomAttributes<SugarIndex>();
            foreach (var index in indexes)
            {
                Assert.True(index.IsUnique,
                    $"Index '{index.IndexName}' on {type.Name} should be marked as unique");
            }
        }
    }

    [Fact]
    public void User_Unique_Indexes_Should_Include_TenantId()
    {
        var indexes = typeof(User).GetCustomAttributes<SugarIndex>();

        foreach (var index in indexes)
        {
            Assert.Contains("TenantId", index.IndexFields.Keys);
        }
    }

    [Fact]
    public void Role_Unique_Indexes_Should_Include_TenantId()
    {
        var indexes = typeof(Role).GetCustomAttributes<SugarIndex>();

        foreach (var index in indexes)
        {
            Assert.Contains("TenantId", index.IndexFields.Keys);
        }
    }

    [Fact]
    public void Dict_Unique_Indexes_Should_Include_TenantId()
    {
        var indexes = typeof(Dict).GetCustomAttributes<SugarIndex>();

        foreach (var index in indexes)
        {
            Assert.Contains("TenantId", index.IndexFields.Keys);
        }
    }

    [Fact]
    public void SystemSetting_Unique_Indexes_Should_Include_TenantId()
    {
        var indexes = typeof(SystemSetting).GetCustomAttributes<SugarIndex>();

        foreach (var index in indexes)
        {
            Assert.Contains("TenantId", index.IndexFields.Keys);
        }
    }

    [Fact]
    public void Entities_With_ITenant_Should_Have_TenantId_Property()
    {
        var assembly = typeof(User).Assembly;
        var tenantEntityTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        t.GetInterfaces().Contains(typeof(Agility.Zoey.Core.Interfaces.ITenant)));

        foreach (var type in tenantEntityTypes)
        {
            var tenantIdProp = type.GetProperty("TenantId");
            Assert.NotNull(tenantIdProp);
            Assert.Equal(typeof(long), tenantIdProp!.PropertyType);
        }
    }

    [Fact]
    public void User_UserName_Should_Not_Be_Nullable()
    {
        var userNameType = typeof(User).GetProperty("UserName")!.PropertyType;

        Assert.Equal(typeof(string), userNameType);
    }

    [Fact]
    public void User_Password_Should_Not_Be_Nullable()
    {
        var passwordType = typeof(User).GetProperty("Password")!.PropertyType;

        Assert.Equal(typeof(string), passwordType);
    }

    [Fact]
    public void Entities_With_ISoftDelete_Should_Have_IsDeleted_Property()
    {
        var assembly = typeof(User).Assembly;
        var softDeleteTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        t.GetInterfaces().Contains(typeof(Agility.Zoey.Core.Interfaces.ISoftDelete)));

        foreach (var type in softDeleteTypes)
        {
            var isDeletedProp = type.GetProperty("IsDeleted");
            Assert.NotNull(isDeletedProp);
            Assert.Equal(typeof(bool), isDeletedProp!.PropertyType);
        }
    }

    [Fact]
    public void RoleMenu_Should_Have_Composite_Primary_Key()
    {
        var roleIdProp = typeof(RoleMenu).GetProperty("RoleId");
        var menuIdProp = typeof(RoleMenu).GetProperty("MenuId");

        var roleIdAttr = roleIdProp!.GetCustomAttribute<SugarColumn>();
        var menuIdAttr = menuIdProp!.GetCustomAttribute<SugarColumn>();

        Assert.NotNull(roleIdAttr);
        Assert.True(roleIdAttr.IsPrimaryKey);
        Assert.NotNull(menuIdAttr);
        Assert.True(menuIdAttr.IsPrimaryKey);
    }

    [Fact]
    public void UserRole_Should_Have_Composite_Primary_Key()
    {
        var userIdProp = typeof(UserRole).GetProperty("UserId");
        var roleIdProp = typeof(UserRole).GetProperty("RoleId");

        var userIdAttr = userIdProp!.GetCustomAttribute<SugarColumn>();
        var roleIdAttr = roleIdProp!.GetCustomAttribute<SugarColumn>();

        Assert.NotNull(userIdAttr);
        Assert.True(userIdAttr.IsPrimaryKey);
        Assert.NotNull(roleIdAttr);
        Assert.True(roleIdAttr.IsPrimaryKey);
    }

    [Fact]
    public void BaseEntity_Should_Have_Id_As_PrimaryKey_And_Identity()
    {
        var idProp = typeof(BaseEntity).GetProperty("Id");
        var sugarColumn = idProp!.GetCustomAttribute<SugarColumn>();

        Assert.NotNull(sugarColumn);
        Assert.True(sugarColumn.IsPrimaryKey);
        Assert.True(sugarColumn.IsIdentity);
    }
}