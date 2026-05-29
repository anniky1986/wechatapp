using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_File")]
public class SysFile : BaseEntity, ITenant
{
    [Required]
    [MaxLength(64)]
    public string FileId { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? OriginalName { get; set; }

    [Required]
    [MaxLength(256)]
    public string StorageName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ContentType { get; set; }

    public long Size { get; set; }

    [MaxLength(128)]
    public string? FileHash { get; set; }

    [Required]
    [MaxLength(512)]
    public string StoragePath { get; set; } = string.Empty;

    public int StorageProvider { get; set; }

    [MaxLength(64)]
    public string? Category { get; set; }

    public long? FolderId { get; set; }

    [MaxLength(1024)]
    public string? ExtendData { get; set; }

    public long UploadUserId { get; set; }

    public long TenantId { get; set; }

    public int Status { get; set; } = 1;
}