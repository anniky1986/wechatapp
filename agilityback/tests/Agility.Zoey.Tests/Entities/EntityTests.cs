using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Tests.Entities;

public class EntityTests
{
    [Fact]
    public void BaseEntity_Should_Have_Correct_Properties()
    {
        var type = typeof(BaseEntity);

        var idProp = type.GetProperty("Id");
        Assert.NotNull(idProp);
        Assert.Equal(typeof(long), idProp.PropertyType);

        var sugarColumn = idProp.GetCustomAttribute<SugarColumn>();
        Assert.NotNull(sugarColumn);
        Assert.True(sugarColumn.IsPrimaryKey);
        Assert.True(sugarColumn.IsIdentity);

        var createTimeProp = type.GetProperty("CreateTime");
        Assert.NotNull(createTimeProp);
        Assert.Equal(typeof(DateTime), createTimeProp.PropertyType);

        var createUserIdProp = type.GetProperty("CreateUserId");
        Assert.NotNull(createUserIdProp);
        Assert.Equal(typeof(long), createUserIdProp.PropertyType);

        var updateTimeProp = type.GetProperty("UpdateTime");
        Assert.NotNull(updateTimeProp);
        Assert.Equal(typeof(DateTime?), updateTimeProp.PropertyType);

        var updateUserIdProp = type.GetProperty("UpdateUserId");
        Assert.NotNull(updateUserIdProp);
        Assert.Equal(typeof(long?), updateUserIdProp.PropertyType);
    }

    [Fact]
    public void BaseEntity_Should_Be_Abstract()
    {
        Assert.True(typeof(BaseEntity).IsAbstract);
    }

    [Fact]
    public void User_Should_Implement_ISoftDelete_And_ITenant()
    {
        var user = new User();

        Assert.IsAssignableFrom<ISoftDelete>(user);
        Assert.IsAssignableFrom<ITenant>(user);
        Assert.IsAssignableFrom<BaseEntity>(user);
    }

    [Fact]
    public void User_Should_Have_SoftDelete_Property()
    {
        var user = new User();
        user.IsDeleted = true;

        Assert.True(((ISoftDelete)user).IsDeleted);
    }

    [Fact]
    public void User_Should_Have_Tenant_Property()
    {
        var user = new User();
        user.TenantId = 99;

        Assert.Equal(99, ((ITenant)user).TenantId);
    }

    [Theory]
    [InlineData(typeof(User), "Sys_User")]
    [InlineData(typeof(Role), "Sys_Role")]
    [InlineData(typeof(Tenant), "Sys_Tenant")]
    [InlineData(typeof(Dict), "Sys_Dict")]
    [InlineData(typeof(DictItem), "Sys_DictItem")]
    [InlineData(typeof(SystemSetting), "Sys_SystemSetting")]
    [InlineData(typeof(Menu), "Sys_Menu")]
    [InlineData(typeof(RoleMenu), "Sys_RoleMenu")]
    [InlineData(typeof(UserRole), "Sys_UserRole")]
    [InlineData(typeof(Dept), "Sys_Dept")]
    [InlineData(typeof(LoginLog), "Sys_LoginLog")]
    [InlineData(typeof(OperationLog), "Sys_OperationLog")]
    [InlineData(typeof(Article), "Sys_Article")]
    [InlineData(typeof(DataPermission), "Sys_DataPermission")]
    [InlineData(typeof(SysFile), "Sys_File")]
    [InlineData(typeof(SysFileFolder), "Sys_FileFolder")]
    [InlineData(typeof(WorkflowDefinition), "Sys_WorkflowDefinition")]
    public void Entity_Should_Have_Correct_SugarTable_Attribute(Type entityType, string expectedTableName)
    {
        var attribute = entityType.GetCustomAttribute<SugarTable>();

        Assert.NotNull(attribute);
        Assert.Equal(expectedTableName, attribute.TableName);
    }

    [Fact]
    public void User_Should_Have_UniqueTenantUsername_Index()
    {
        var indexes = typeof(User).GetCustomAttributes<SugarIndex>();

        var index = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_username");

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void User_Should_Have_UniqueTenantEmail_Index()
    {
        var indexes = typeof(User).GetCustomAttributes<SugarIndex>();

        var index = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_email");

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Role_Should_Have_UniqueTenantCode_Index()
    {
        var indexes = typeof(Role).GetCustomAttributes<SugarIndex>();

        var index = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_code");

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Tenant_Should_Have_UniqueCode_Index()
    {
        var indexes = typeof(Tenant).GetCustomAttributes<SugarIndex>();

        var index = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_code");

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Dict_Should_Have_UniqueTenantCode_Index()
    {
        var indexes = typeof(Dict).GetCustomAttributes<SugarIndex>();

        var index = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_code");

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void SystemSetting_Should_Have_UniqueTenantConfigKey_Index()
    {
        var indexes = typeof(SystemSetting).GetCustomAttributes<SugarIndex>();

        var index = indexes.FirstOrDefault(i =>
            i.IndexName == "unique_tenant_configkey");

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void User_Should_Have_Required_And_MaxLength_Attributes()
    {
        var userNameProp = typeof(User).GetProperty("UserName");
        Assert.NotNull(userNameProp);
        Assert.NotNull(userNameProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, userNameProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);

        var passwordProp = typeof(User).GetProperty("Password");
        Assert.NotNull(passwordProp);
        Assert.NotNull(passwordProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(256, passwordProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);
    }

    [Fact]
    public void Role_Should_Have_Required_Name_And_Code()
    {
        var nameProp = typeof(Role).GetProperty("Name");
        Assert.NotNull(nameProp);
        Assert.NotNull(nameProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, nameProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);

        var codeProp = typeof(Role).GetProperty("Code");
        Assert.NotNull(codeProp);
        Assert.NotNull(codeProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, codeProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);
    }

    [Fact]
    public void Tenant_Should_Have_Required_Name_And_Code()
    {
        var nameProp = typeof(Tenant).GetProperty("Name");
        Assert.NotNull(nameProp);
        Assert.NotNull(nameProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, nameProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);

        var codeProp = typeof(Tenant).GetProperty("Code");
        Assert.NotNull(codeProp);
        Assert.NotNull(codeProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, codeProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);
    }

    [Fact]
    public void Dict_Should_Have_Required_Name_And_Code()
    {
        var nameProp = typeof(Dict).GetProperty("Name");
        Assert.NotNull(nameProp);
        Assert.NotNull(nameProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, nameProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);

        var codeProp = typeof(Dict).GetProperty("Code");
        Assert.NotNull(codeProp);
        Assert.NotNull(codeProp.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(64, codeProp.GetCustomAttribute<MaxLengthAttribute>()?.Length);
    }

    [Fact]
    public void Article_Should_Implement_ISoftDelete_And_ITenant()
    {
        var article = new Article();

        Assert.IsAssignableFrom<ISoftDelete>(article);
        Assert.IsAssignableFrom<ITenant>(article);
    }

    [Fact]
    public void Role_Should_Implement_ITenant()
    {
        var role = new Role();

        Assert.IsAssignableFrom<ITenant>(role);
    }

    [Fact]
    public void Role_Should_Not_Implement_ISoftDelete()
    {
        Assert.False(typeof(Role).GetInterfaces().Contains(typeof(ISoftDelete)));
    }
}