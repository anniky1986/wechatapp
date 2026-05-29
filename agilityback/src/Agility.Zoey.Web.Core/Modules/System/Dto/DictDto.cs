namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record DictPageInput : PageInput
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}

public record AddDictInput
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public record UpdateDictInput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public record DictOutput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}

public record AddDictItemInput
{
    public long DictId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Sort { get; set; }
    public bool IsDefault { get; set; }
    public int Status { get; set; } = 1;
}

public record UpdateDictItemInput
{
    public long Id { get; set; }
    public long DictId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Sort { get; set; }
    public bool IsDefault { get; set; }
    public int Status { get; set; } = 1;
}

public record DictItemOutput
{
    public long Id { get; set; }
    public long DictId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Sort { get; set; }
    public bool IsDefault { get; set; }
    public int Status { get; set; }
}