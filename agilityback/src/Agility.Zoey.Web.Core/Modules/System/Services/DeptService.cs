using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Furion.DynamicApiController;
using Furion.FriendlyException;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 130)]
public class DeptService : IDynamicApiController, ITransient
{
    private readonly IRepository<Dept> _deptRepo;

    public DeptService(IRepository<Dept> deptRepo)
    {
        _deptRepo = deptRepo;
    }

    [HttpGet("api/dept/tree")]
    public async Task<List<DeptTreeOutput>> GetTree()
    {
        var depts = await _deptRepo.AsQueryable()
            .OrderBy(d => d.Sort)
            .ToListAsync();

        return BuildDeptTree(depts, 0);
    }

    [HttpGet("api/dept/list")]
    public async Task<List<DeptOutput>> GetList()
    {
        var depts = await _deptRepo.AsQueryable()
            .Where(d => d.Status == 1)
            .OrderBy(d => d.Sort)
            .ToListAsync();

        return depts.Select(d => new DeptOutput
        {
            Id = d.Id,
            ParentId = d.ParentId,
            Name = d.Name,
            Leader = d.Leader,
            Phone = d.Phone,
            Sort = d.Sort,
            Status = d.Status,
            CreateTime = d.CreateTime
        }).ToList();
    }

    [HttpGet("api/dept/{id}")]
    public async Task<DeptOutput> Get(long id)
    {
        var dept = await _deptRepo.GetByIdAsync(id);
        if (dept == null)
        {
            throw Oops.Oh("部门不存在");
        }

        return new DeptOutput
        {
            Id = dept.Id,
            ParentId = dept.ParentId,
            Name = dept.Name,
            Leader = dept.Leader,
            Phone = dept.Phone,
            Sort = dept.Sort,
            Status = dept.Status,
            CreateTime = dept.CreateTime
        };
    }

    [HttpPost("api/dept")]
    public async Task<DeptOutput> Add(AddDeptInput input)
    {
        var dept = new Dept
        {
            ParentId = input.ParentId,
            Name = input.Name,
            Leader = input.Leader,
            Phone = input.Phone,
            Sort = input.Sort,
            Status = input.Status,
            TenantId = 0,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        dept = await _deptRepo.InsertAsync(dept);
        return await Get(dept.Id);
    }

    [HttpPut("api/dept/{id}")]
    public async Task<DeptOutput> Update(long id, UpdateDeptInput input)
    {
        var dept = await _deptRepo.GetByIdAsync(id);
        if (dept == null)
        {
            throw Oops.Oh("部门不存在");
        }

        if (input.ParentId == id)
        {
            throw Oops.Oh("不能将部门的父级设置为自身");
        }

        dept.ParentId = input.ParentId;
        dept.Name = input.Name;
        dept.Leader = input.Leader;
        dept.Phone = input.Phone;
        dept.Sort = input.Sort;
        dept.Status = input.Status;
        dept.UpdateTime = DateTime.Now;
        dept.UpdateUserId = 0;

        await _deptRepo.UpdateAsync(dept);
        return await Get(id);
    }

    [HttpDelete("api/dept/{id}")]
    public async Task Delete(long id)
    {
        var dept = await _deptRepo.GetByIdAsync(id);
        if (dept == null)
        {
            throw Oops.Oh("部门不存在");
        }

        var hasChildren = await _deptRepo.AsQueryable()
            .AnyAsync(d => d.ParentId == id);
        if (hasChildren)
        {
            throw Oops.Oh("存在子部门，请先删除子部门");
        }

        await _deptRepo.DeleteAsync(id);
    }

    private static List<DeptTreeOutput> BuildDeptTree(List<Dept> depts, long parentId)
    {
        return depts
            .Where(d => d.ParentId == parentId)
            .OrderBy(d => d.Sort)
            .Select(d => new DeptTreeOutput
            {
                Id = d.Id,
                ParentId = d.ParentId,
                Name = d.Name,
                Leader = d.Leader,
                Phone = d.Phone,
                Sort = d.Sort,
                Status = d.Status,
                Children = BuildDeptTree(depts, d.Id)
            })
            .ToList();
    }
}