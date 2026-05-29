namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record AddDeptInput
{
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
}

public record UpdateDeptInput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
}

public record DeptOutput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; }
    public DateTime CreateTime { get; set; }
}

public record DeptTreeOutput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; }
    public List<DeptTreeOutput> Children { get; set; } = new();
}