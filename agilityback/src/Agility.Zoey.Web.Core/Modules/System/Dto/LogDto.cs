namespace Agility.Zoey.Web.Core.Modules.System.Dto;

public record LogPageInput : PageInput
{
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public record OperationLogOutput
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? Module { get; set; }
    public string? Description { get; set; }
    public string? RequestUrl { get; set; }
    public string? Method { get; set; }
    public string? RequestParams { get; set; }
    public string? ResponseResult { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExecutionTime { get; set; }
    public long DurationMs { get; set; }
}

public record LoginLogOutput
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public int LoginType { get; set; }
    public int Status { get; set; }
    public string? Message { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; }
}