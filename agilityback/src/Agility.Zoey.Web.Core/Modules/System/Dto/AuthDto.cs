namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record LoginInput
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record ChangePasswordInput
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public record UserInfo
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? Avatar { get; set; }
    public long DeptId { get; set; }
    public string? DeptName { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public record LoginOutput
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserInfo UserInfo { get; set; } = new();
    public List<MenuTreeOutput> Menus { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public record InitOutput
{
    public UserInfo UserInfo { get; set; } = new();
    public List<MenuTreeOutput> Menus { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, string> SystemConfig { get; set; } = new();
    public Dictionary<string, List<DictItemOutput>> DictData { get; set; } = new();
}