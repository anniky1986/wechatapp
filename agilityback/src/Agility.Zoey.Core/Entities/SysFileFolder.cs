using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_FileFolder")]
public class SysFileFolder : BaseEntity, ITenant
{
    public long ParentId { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? Color { get; set; }

    [MaxLength(128)]
    public string? Icon { get; set; }

    public int Sort { get; set; }

    public long CreateUserId { get; set; }

    public long TenantId { get; set; }
}