using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_DataPermission")]
public class DataPermission : BaseEntity, ITenant
{
    public long RoleId { get; set; }

    public long DeptId { get; set; }

    public long TenantId { get; set; }
}