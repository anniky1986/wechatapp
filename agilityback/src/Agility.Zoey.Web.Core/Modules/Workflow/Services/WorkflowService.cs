using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Furion.DynamicApiController;
using Furion.FriendlyException;

namespace Agility.Zoey.Web.Core.Modules.Workflow.Services;

[ApiDescriptionSettings(Group = "Workflow", Order = 400)]
public class WorkflowService : IDynamicApiController, ITransient
{
    private readonly IRepository<WorkflowDefinition> _definitionRepo;

    public WorkflowService(IRepository<WorkflowDefinition> definitionRepo)
    {
        _definitionRepo = definitionRepo;
    }

    [HttpGet("api/workflow/definitions")]
    public async Task<List<WorkflowDefinitionOutput>> GetDefinitions()
    {
        var definitions = await _definitionRepo.AsQueryable()
            .OrderBy(d => d.CreateTime)
            .ToListAsync();

        return definitions.Select(d => new WorkflowDefinitionOutput
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            Config = d.Config,
            Status = d.Status,
            CreateTime = d.CreateTime
        }).ToList();
    }

    [HttpPost("api/workflow/instance")]
    public async Task<WorkflowInstanceOutput> CreateInstance(CreateWorkflowInstanceInput input)
    {
        var definition = await _definitionRepo.GetByIdAsync(input.DefinitionId);
        if (definition == null)
        {
            throw Oops.Oh("工作流定义不存在");
        }

        return new WorkflowInstanceOutput
        {
            InstanceId = Guid.NewGuid().ToString("N"),
            DefinitionId = definition.Id,
            DefinitionName = definition.Name,
            Status = "Pending",
            CreateTime = DateTime.Now
        };
    }

    [HttpPost("api/workflow/approve")]
    public async Task<WorkflowResultOutput> Approve(WorkflowActionInput input)
    {
        return new WorkflowResultOutput
        {
            Success = true,
            Message = "审批通过"
        };
    }

    [HttpPost("api/workflow/reject")]
    public async Task<WorkflowResultOutput> Reject(WorkflowActionInput input)
    {
        return new WorkflowResultOutput
        {
            Success = true,
            Message = "已驳回"
        };
    }
}

public record WorkflowDefinitionOutput
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Config { get; set; }
    public int Status { get; set; }
    public DateTime CreateTime { get; set; }
}

public record CreateWorkflowInstanceInput
{
    public long DefinitionId { get; set; }
}

public record WorkflowActionInput
{
    public string InstanceId { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public record WorkflowInstanceOutput
{
    public string InstanceId { get; set; } = string.Empty;
    public long DefinitionId { get; set; }
    public string DefinitionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

public record WorkflowResultOutput
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}