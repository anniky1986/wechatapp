using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_WorkflowDefinition")]
public class WorkflowDefinition : BaseEntity, ITenant
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "text")]
    public string? Config { get; set; }

    public int Status { get; set; } = 1;

    public long TenantId { get; set; }
}