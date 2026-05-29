using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_Article")]
public class Article : BaseEntity, ITenant, ISoftDelete
{
    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Category { get; set; }

    public int Status { get; set; } = 1;

    public long TenantId { get; set; }

    public bool IsDeleted { get; set; }
}