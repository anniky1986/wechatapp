namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record UpdateSettingInput
{
    public string ConfigKey { get; set; } = string.Empty;
    public string? ConfigValue { get; set; }
}

public record SettingOutput
{
    public long Id { get; set; }
    public string ConfigKey { get; set; } = string.Empty;
    public string? ConfigValue { get; set; }
    public string ValueType { get; set; } = "String";
    public string? Group { get; set; }
    public string? Description { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; }
}