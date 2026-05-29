using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_SystemSetting")]
public class SystemSetting : BaseEntity, ITenant
{
    public long TenantId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ConfigKey { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? ConfigValue { get; set; }

    [MaxLength(32)]
    public string ValueType { get; set; } = "String";

    [MaxLength(64)]
    public string? Group { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    public bool IsCacheable { get; set; }

    public int Sort { get; set; }

    public int Status { get; set; } = 1;
}