using System.Text;
using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using SqlSugar;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 170)]
public class LogService : IDynamicApiController, ITransient
{
    private readonly IRepository<OperationLog> _operationLogRepo;
    private readonly IRepository<LoginLog> _loginLogRepo;

    public LogService(
        IRepository<OperationLog> operationLogRepo,
        IRepository<LoginLog> loginLogRepo)
    {
        _operationLogRepo = operationLogRepo;
        _loginLogRepo = loginLogRepo;
    }

    [HttpGet("api/log/operation/page")]
    [Permission(PermissionConsts.LogView)]
    public async Task<PageResult<OperationLogOutput>> GetOperationLogPage(LogPageInput input)
    {
        var query = _operationLogRepo.AsQueryable();

        if (input.UserId.HasValue)
        {
            query = query.Where(l => l.UserId == input.UserId.Value);
        }

        if (!string.IsNullOrEmpty(input.UserName))
        {
            query = query.Where(l => l.UserName != null && l.UserName.Contains(input.UserName));
        }

        if (input.StartTime.HasValue)
        {
            query = query.Where(l => l.ExecutionTime >= input.StartTime.Value);
        }

        if (input.EndTime.HasValue)
        {
            query = query.Where(l => l.ExecutionTime <= input.EndTime.Value);
        }

        var page = input.Page < 1 ? 1 : input.Page;
        var pageSize = input.PageSize < 1 ? SystemConsts.DefaultPageSize : input.PageSize;
        if (pageSize > SystemConsts.MaxPageSize) pageSize = SystemConsts.MaxPageSize;

        query = query.OrderBy(l => l.Id, OrderByType.Desc);

        var totalCount = await query.CountAsync();
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = logs.Select(l => new OperationLogOutput
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.UserName,
            Module = l.Module,
            Description = l.Description,
            RequestUrl = l.RequestUrl,
            Method = l.Method,
            RequestParams = l.RequestParams,
            ResponseResult = l.ResponseResult,
            IpAddress = l.IpAddress,
            ExecutionTime = l.ExecutionTime,
            DurationMs = l.DurationMs
        }).ToList();

        return new PageResult<OperationLogOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/log/login/page")]
    [Permission(PermissionConsts.LogView)]
    public async Task<PageResult<LoginLogOutput>> GetLoginLogPage(LogPageInput input)
    {
        var query = _loginLogRepo.AsQueryable();

        if (input.UserId.HasValue)
        {
            query = query.Where(l => l.UserId == input.UserId.Value);
        }

        if (!string.IsNullOrEmpty(input.UserName))
        {
            query = query.Where(l => l.UserName != null && l.UserName.Contains(input.UserName));
        }

        if (input.StartTime.HasValue)
        {
            query = query.Where(l => l.LoginTime >= input.StartTime.Value);
        }

        if (input.EndTime.HasValue)
        {
            query = query.Where(l => l.LoginTime <= input.EndTime.Value);
        }

        var page = input.Page < 1 ? 1 : input.Page;
        var pageSize = input.PageSize < 1 ? SystemConsts.DefaultPageSize : input.PageSize;
        if (pageSize > SystemConsts.MaxPageSize) pageSize = SystemConsts.MaxPageSize;

        query = query.OrderBy(l => l.Id, OrderByType.Desc);

        var totalCount = await query.CountAsync();
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = logs.Select(l => new LoginLogOutput
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.UserName,
            LoginType = l.LoginType,
            Status = l.Status,
            Message = l.Message,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            LoginTime = l.LoginTime
        }).ToList();

        return new PageResult<LoginLogOutput>
        {
            Total = totalCount,
            Items = items
        };
    }

    [HttpGet("api/log/operation/export")]
    [Permission(PermissionConsts.LogExport)]
    public async Task<IActionResult> ExportOperationLog(LogPageInput input)
    {
        input.Page = 1;
        input.PageSize = int.MaxValue;
        var pageResult = await GetOperationLogPage(input);
        var json = System.Text.Json.JsonSerializer.Serialize(pageResult.Items);
        var bytes = Encoding.UTF8.GetBytes(json);
        return new FileContentResult(bytes, "application/json")
        {
            FileDownloadName = $"operation_logs_{DateTime.Now:yyyyMMddHHmmss}.json"
        };
    }

    [HttpGet("api/log/login/export")]
    [Permission(PermissionConsts.LogExport)]
    public async Task<IActionResult> ExportLoginLog(LogPageInput input)
    {
        input.Page = 1;
        input.PageSize = int.MaxValue;
        var pageResult = await GetLoginLogPage(input);
        var json = System.Text.Json.JsonSerializer.Serialize(pageResult.Items);
        var bytes = Encoding.UTF8.GetBytes(json);
        return new FileContentResult(bytes, "application/json")
        {
            FileDownloadName = $"login_logs_{DateTime.Now:yyyyMMddHHmmss}.json"
        };
    }
}