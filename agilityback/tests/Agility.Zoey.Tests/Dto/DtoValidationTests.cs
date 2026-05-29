using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Web.Core.Modules.System.Dto;

namespace Agility.Zoey.Tests.Dto;

public class DtoValidationTests
{
    [Fact]
    public void AddUserInput_Should_Require_UserName()
    {
        var input = new AddUserInput
        {
            UserName = string.Empty,
            Password = "Test@123"
        };

        Assert.Equal(string.Empty, input.UserName);
        Assert.True(string.IsNullOrEmpty(input.UserName));
    }

    [Fact]
    public void AddUserInput_Should_Require_Password()
    {
        var input = new AddUserInput
        {
            UserName = "testuser",
            Password = string.Empty
        };

        Assert.Equal(string.Empty, input.Password);
        Assert.True(string.IsNullOrEmpty(input.Password));
    }

    [Fact]
    public void AddUserInput_Should_Have_Default_Values()
    {
        var input = new AddUserInput
        {
            UserName = "testuser",
            Password = "Test@123"
        };

        Assert.Equal(1, input.Status);
        Assert.NotNull(input.RoleIds);
        Assert.Empty(input.RoleIds);
    }

    [Fact]
    public void AddUserInput_Should_Accept_Optional_Fields()
    {
        var input = new AddUserInput
        {
            UserName = "testuser",
            Password = "Test@123",
            NickName = "Test User",
            Email = "test@example.com",
            Phone = "13800138000",
            DeptId = 5,
            Status = 1,
            RoleIds = new List<long> { 1, 2 }
        };

        Assert.Equal("Test User", input.NickName);
        Assert.Equal("test@example.com", input.Email);
        Assert.Equal("13800138000", input.Phone);
        Assert.Equal(5, input.DeptId);
        Assert.Equal(2, input.RoleIds.Count);
    }

    [Fact]
    public void AddRoleInput_Should_Require_Name()
    {
        var input = new AddRoleInput
        {
            Name = string.Empty,
            Code = "test_role"
        };

        Assert.True(string.IsNullOrEmpty(input.Name));
    }

    [Fact]
    public void AddRoleInput_Should_Require_Code()
    {
        var input = new AddRoleInput
        {
            Name = "Test Role",
            Code = string.Empty
        };

        Assert.True(string.IsNullOrEmpty(input.Code));
    }

    [Fact]
    public void AddRoleInput_Should_Have_Default_Values()
    {
        var input = new AddRoleInput
        {
            Name = "Test Role",
            Code = "test_role"
        };

        Assert.Equal(1, input.Status);
        Assert.Equal(1, input.DataScope);
        Assert.NotNull(input.MenuIds);
        Assert.Empty(input.MenuIds);
    }

    [Fact]
    public void UpdateUserInput_Should_Have_Default_Status()
    {
        var input = new UpdateUserInput();

        Assert.Equal(1, input.Status);
        Assert.NotNull(input.RoleIds);
        Assert.Empty(input.RoleIds);
    }

    [Fact]
    public void UpdateRoleInput_Should_Have_Default_Values()
    {
        var input = new UpdateRoleInput
        {
            Name = "Updated Role",
            Code = "updated_role"
        };

        Assert.Equal(1, input.Status);
        Assert.Equal(1, input.DataScope);
        Assert.NotNull(input.MenuIds);
        Assert.Empty(input.MenuIds);
    }

    [Fact]
    public void User_Entity_Should_Fail_Validation_When_UserName_Empty()
    {
        var user = new User
        {
            UserName = string.Empty,
            Password = "Test@123"
        };

        var results = ValidateModel(user);

        Assert.Contains(results, r => r.MemberNames.Contains("UserName"));
    }

    [Fact]
    public void User_Entity_Should_Fail_Validation_When_Password_Empty()
    {
        var user = new User
        {
            UserName = "testuser",
            Password = string.Empty
        };

        var results = ValidateModel(user);

        Assert.Contains(results, r => r.MemberNames.Contains("Password"));
    }

    [Fact]
    public void User_Entity_Should_Pass_Validation_When_All_Required_Filled()
    {
        var user = new User
        {
            UserName = "testuser",
            Password = "Test@123"
        };

        var results = ValidateModel(user);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("UserName", 64)]
    [InlineData("Password", 256)]
    [InlineData("NickName", 64)]
    [InlineData("Email", 256)]
    [InlineData("Phone", 20)]
    [InlineData("Avatar", 512)]
    public void User_Entity_Should_Have_Correct_String_Length_Constraints(string propertyName, int maxLength)
    {
        var prop = typeof(User).GetProperty(propertyName);
        Assert.NotNull(prop);

        var maxLengthAttr = prop.GetCustomAttributes(typeof(MaxLengthAttribute), true)
            .FirstOrDefault() as MaxLengthAttribute;

        if (propertyName == "NickName" || propertyName == "Email" || propertyName == "Phone" || propertyName == "Avatar")
        {
            Assert.NotNull(maxLengthAttr);
            Assert.Equal(maxLength, maxLengthAttr!.Length);
        }
        else
        {
            Assert.NotNull(maxLengthAttr);
            Assert.Equal(maxLength, maxLengthAttr!.Length);
        }
    }

    [Fact]
    public void Role_Entity_Should_Pass_Validation_When_All_Required_Filled()
    {
        var role = new Role
        {
            Name = "Test Role",
            Code = "test_role"
        };

        var results = ValidateModel(role);

        Assert.Empty(results);
    }

    [Fact]
    public void Role_Entity_Should_Fail_Validation_When_Name_Empty()
    {
        var role = new Role
        {
            Name = string.Empty,
            Code = "test_role"
        };

        var results = ValidateModel(role);

        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Role_Entity_Should_Fail_Validation_When_Code_Empty()
    {
        var role = new Role
        {
            Name = "Test Role",
            Code = string.Empty
        };

        var results = ValidateModel(role);

        Assert.Contains(results, r => r.MemberNames.Contains("Code"));
    }

    [Theory]
    [InlineData(typeof(Role), "Name", 64)]
    [InlineData(typeof(Role), "Code", 64)]
    [InlineData(typeof(Role), "Remark", 256)]
    public void Role_Entity_Should_Have_Correct_String_Length_Constraints(Type entityType, string propertyName, int maxLength)
    {
        var prop = entityType.GetProperty(propertyName);
        Assert.NotNull(prop);

        var maxLengthAttr = prop.GetCustomAttributes(typeof(MaxLengthAttribute), true)
            .FirstOrDefault() as MaxLengthAttribute;

        Assert.NotNull(maxLengthAttr);
        Assert.Equal(maxLength, maxLengthAttr!.Length);
    }

    [Fact]
    public void Tenant_Entity_Should_Have_String_Length_Constraints()
    {
        var nameProp = typeof(Tenant).GetProperty("Name");
        var codeProp = typeof(Tenant).GetProperty("Code");

        var nameLength = nameProp!.GetCustomAttributes(typeof(MaxLengthAttribute), true)
            .FirstOrDefault() as MaxLengthAttribute;
        var codeLength = codeProp!.GetCustomAttributes(typeof(MaxLengthAttribute), true)
            .FirstOrDefault() as MaxLengthAttribute;

        Assert.NotNull(nameLength);
        Assert.Equal(64, nameLength!.Length);
        Assert.NotNull(codeLength);
        Assert.Equal(64, codeLength!.Length);
    }

    [Fact]
    public void Dict_Entity_Should_Have_String_Length_Constraints()
    {
        var nameProp = typeof(Dict).GetProperty("Name");
        var codeProp = typeof(Dict).GetProperty("Code");
        var remarkProp = typeof(Dict).GetProperty("Remark");

        Assert.Equal(64, ((MaxLengthAttribute)nameProp!.GetCustomAttributes(typeof(MaxLengthAttribute), true).First()).Length);
        Assert.Equal(64, ((MaxLengthAttribute)codeProp!.GetCustomAttributes(typeof(MaxLengthAttribute), true).First()).Length);
        Assert.Equal(256, ((MaxLengthAttribute)remarkProp!.GetCustomAttributes(typeof(MaxLengthAttribute), true).First()).Length);
    }

    [Fact]
    public void PageInput_Should_Have_Default_Page_Values()
    {
        var input = new PageInput();

        Assert.Equal(1, input.Page);
        Assert.Equal(20, input.PageSize);
    }

    [Fact]
    public void PageResult_Should_Return_Empty_List_By_Default()
    {
        var result = new PageResult<UserOutput>();

        Assert.Equal(0, result.Total);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void UserOutput_Should_Have_Default_Empty_Collections()
    {
        var output = new UserOutput();

        Assert.NotNull(output.RoleNames);
        Assert.Empty(output.RoleNames);
        Assert.NotNull(output.RoleIds);
        Assert.Empty(output.RoleIds);
    }

    [Fact]
    public void RoleOutput_Should_Have_Default_Empty_MenuIds()
    {
        var output = new RoleOutput();

        Assert.NotNull(output.MenuIds);
        Assert.Empty(output.MenuIds);
    }

    [Fact]
    public void AddUserInput_With_Empty_RoleIds_Should_Be_Valid()
    {
        var input = new AddUserInput
        {
            UserName = "testuser",
            Password = "Test@123"
        };

        Assert.Empty(input.RoleIds);
        Assert.Equal("testuser", input.UserName);
        Assert.Equal("Test@123", input.Password);
    }

    [Fact]
    public void AddRoleInput_With_Empty_MenuIds_Should_Be_Valid()
    {
        var input = new AddRoleInput
        {
            Name = "Test Role",
            Code = "test_role"
        };

        Assert.Empty(input.MenuIds);
        Assert.Equal("Test Role", input.Name);
        Assert.Equal("test_role", input.Code);
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }
}