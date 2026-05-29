namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record CommonDto;

public record PageInput
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
}

public record PageResult<T>
{
    public int Total { get; set; }
    public List<T> Items { get; set; } = new();
}

public record IdInput
{
    public long Id { get; set; }
}