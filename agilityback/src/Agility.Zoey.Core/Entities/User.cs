using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_User")]
[SugarIndex("unique_tenant_username", nameof(TenantId), OrderByType.Asc, nameof(UserName), OrderByType.Asc, true)]
[SugarIndex("unique_tenant_email", nameof(TenantId), OrderByType.Asc, nameof(Email), OrderByType.Asc, true)]
public class User : BaseEntity, ISoftDelete, ITenant
{
    [Required]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? NickName { get; set; }

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(20)]
    [Phone]
    public string? Phone { get; set; }

    [MaxLength(512)]
    public string? Avatar { get; set; }

    public long DeptId { get; set; }

    public long TenantId { get; set; }

    public int Status { get; set; } = 1;

    public int LoginFailCount { get; set; }

    public DateTime? LockEndTime { get; set; }

    public DateTime? LastLoginTime { get; set; }

    [MaxLength(64)]
    public string? LastLoginIp { get; set; }

    public DateTime? PasswordExpireTime { get; set; }

    public bool IsDeleted { get; set; }
}