using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_DictItem")]
public class DictItem : BaseEntity, ITenant
{
    public long DictId { get; set; }

    public long TenantId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Value { get; set; } = string.Empty;

    public int Sort { get; set; }

    public bool IsDefault { get; set; }

    public int Status { get; set; } = 1;
}