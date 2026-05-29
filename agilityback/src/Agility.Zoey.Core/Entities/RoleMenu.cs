using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_RoleMenu")]
public class RoleMenu
{
    [SugarColumn(IsPrimaryKey = true)]
    public long RoleId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public long MenuId { get; set; }
}