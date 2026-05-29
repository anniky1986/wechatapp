using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_LoginLog")]
public class LoginLog : BaseEntity
{
    public long UserId { get; set; }

    [MaxLength(64)]
    public string? UserName { get; set; }

    public int LoginType { get; set; } = 1;

    public int Status { get; set; } = 1;

    [MaxLength(256)]
    public string? Message { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public DateTime LoginTime { get; set; }
}