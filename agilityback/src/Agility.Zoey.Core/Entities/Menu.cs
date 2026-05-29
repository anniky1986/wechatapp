using System.ComponentModel.DataAnnotations;
using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_Menu")]
public class Menu : BaseEntity, ITenant
{
    public long ParentId { get; set; }

    public long TenantId { get; set; }

    public int MenuType { get; set; }

    [MaxLength(64)]
    public string? Name { get; set; }

    [MaxLength(256)]
    public string? Path { get; set; }

    [MaxLength(256)]
    public string? Component { get; set; }

    [MaxLength(256)]
    public string? Redirect { get; set; }

    [MaxLength(128)]
    public string? Icon { get; set; }

    [MaxLength(256)]
    public string? Permission { get; set; }

    public int OrderNo { get; set; }

    public bool IsHide { get; set; }

    public bool KeepAlive { get; set; }

    public int Status { get; set; } = 1;

    public bool IsFrame { get; set; }

    [MaxLength(512)]
    public string? FrameSrc { get; set; }

    public bool IsObsolete { get; set; }
}