using SqlSugar;

namespace Agility.Zoey.Core.Entities;

public abstract class BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public long CreateUserId { get; set; }

    public DateTime? UpdateTime { get; set; }

    public long? UpdateUserId { get; set; }
}