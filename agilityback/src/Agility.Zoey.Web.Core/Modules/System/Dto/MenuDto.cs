namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record AddMenuInput
{
    public long ParentId { get; set; }
    public int MenuType { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public string? Permission { get; set; }
    public int OrderNo { get; set; }
    public bool IsHide { get; set; }
    public bool KeepAlive { get; set; }
    public int Status { get; set; } = 1;
    public bool IsFrame { get; set; }
    public string? FrameSrc { get; set; }
}

public record UpdateMenuInput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public int MenuType { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public string? Permission { get; set; }
    public int OrderNo { get; set; }
    public bool IsHide { get; set; }
    public bool KeepAlive { get; set; }
    public int Status { get; set; } = 1;
    public bool IsFrame { get; set; }
    public string? FrameSrc { get; set; }
}

public record MenuOutput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public int MenuType { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public string? Permission { get; set; }
    public int OrderNo { get; set; }
    public bool IsHide { get; set; }
    public bool KeepAlive { get; set; }
    public int Status { get; set; }
    public bool IsFrame { get; set; }
    public string? FrameSrc { get; set; }
    public DateTime CreateTime { get; set; }
}

public record MenuTreeOutput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public string? Permission { get; set; }
    public int MenuType { get; set; }
    public int OrderNo { get; set; }
    public bool IsHide { get; set; }
    public bool KeepAlive { get; set; }
    public int Status { get; set; }
    public bool IsFrame { get; set; }
    public string? FrameSrc { get; set; }
    public List<MenuTreeOutput> Children { get; set; } = new();
}