using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_OperationLog")]
public class OperationLog : BaseEntity
{
    public long UserId { get; set; }

    [MaxLength(64)]
    public string? UserName { get; set; }

    [MaxLength(128)]
    public string? Module { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    [MaxLength(512)]
    public string? RequestUrl { get; set; }

    [MaxLength(16)]
    public string? Method { get; set; }

    [MaxLength(4000)]
    public string? RequestParams { get; set; }

    [MaxLength(4000)]
    public string? ResponseResult { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    public DateTime ExecutionTime { get; set; }

    public long DurationMs { get; set; }
}