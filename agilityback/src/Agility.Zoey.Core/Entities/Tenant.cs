using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_Tenant")]
[SugarIndex("unique_code", nameof(Code), OrderByType.Asc, true)]
public class Tenant : BaseEntity
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    public int TenantType { get; set; } = 1;

    [MaxLength(512)]
    public string? DbConnectionString { get; set; }

    public int Status { get; set; } = 1;

    public DateTime? ExpireTime { get; set; }
}