namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record TenantPageInput : PageInput
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int? Status { get; set; }
}

public record AddTenantInput
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int TenantType { get; set; } = 1;
    public string? DbConnectionString { get; set; }
    public int Status { get; set; } = 1;
    public DateTime? ExpireTime { get; set; }
}

public record UpdateTenantInput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int TenantType { get; set; } = 1;
    public string? DbConnectionString { get; set; }
    public int Status { get; set; } = 1;
    public DateTime? ExpireTime { get; set; }
}

public record TenantOutput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int TenantType { get; set; }
    public string? DbConnectionString { get; set; }
    public int Status { get; set; }
    public DateTime? ExpireTime { get; set; }
    public DateTime CreateTime { get; set; }
}