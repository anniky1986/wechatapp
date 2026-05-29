namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record RolePageInput : PageInput
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int? Status { get; set; }
}

public record AddRoleInput
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
    public int DataScope { get; set; } = 1;
    public List<long> MenuIds { get; set; } = new();
}

public record UpdateRoleInput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
    public int DataScope { get; set; } = 1;
    public List<long> MenuIds { get; set; } = new();
}

public record RoleOutput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; }
    public int DataScope { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public List<long> MenuIds { get; set; } = new();
}

public record DataPermissionInput
{
    public List<long> DeptIds { get; set; } = new();
}