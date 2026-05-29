using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_Role")]
public class Role : BaseEntity, ITenant
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    public long TenantId { get; set; }

    public int DataScope { get; set; } = 1;

    [MaxLength(256)]
    public string? Remark { get; set; }

    public int Sort { get; set; }

    public int Status { get; set; } = 1;
}