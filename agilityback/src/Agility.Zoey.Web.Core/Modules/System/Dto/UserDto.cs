namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record UserPageInput : PageInput
{
    public string? UserName { get; set; }
    public string? Phone { get; set; }
    public int? Status { get; set; }
    public long? DeptId { get; set; }
}

public record AddUserInput
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public long DeptId { get; set; }
    public List<long> RoleIds { get; set; } = new();
    public int Status { get; set; } = 1;
}

public record UpdateUserInput
{
    public long Id { get; set; }
    public string? NickName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public long DeptId { get; set; }
    public List<long> RoleIds { get; set; } = new();
    public int Status { get; set; } = 1;
}

public record UserOutput
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public long DeptId { get; set; }
    public string? DeptName { get; set; }
    public int Status { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime CreateTime { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public List<long> RoleIds { get; set; } = new();
}