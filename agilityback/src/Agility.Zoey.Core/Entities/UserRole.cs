using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_UserRole")]
public class UserRole
{
    [SugarColumn(IsPrimaryKey = true)]
    public long UserId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public long RoleId { get; set; }
}