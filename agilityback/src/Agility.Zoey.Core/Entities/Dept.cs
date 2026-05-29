using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_Dept")]
public class Dept : BaseEntity, ITenant
{
    public long ParentId { get; set; }

    public long TenantId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Leader { get; set; }

    [MaxLength(20)]
    [Phone]
    public string? Phone { get; set; }

    public int Sort { get; set; }

    public int Status { get; set; } = 1;
}