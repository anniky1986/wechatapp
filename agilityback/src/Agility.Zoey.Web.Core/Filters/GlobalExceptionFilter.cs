using System.Net;
using System.Text.Json;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Agility.Zoey.Web.Core.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        _logger.LogError(exception, "全局异常捕获: {Message}", exception.Message);

        context.ExceptionHandled = true;

        if (exception is AppFriendlyException friendlyException)
        {
            context.Result = new ObjectResult(new
            {
                code = (int)HttpStatusCode.BadRequest,
                msg = friendlyException.ErrorMessage,
                data = (object?)null
            })
            {
                StatusCode = (int)HttpStatusCode.OK
            };
            return;
        }

        context.Result = new ObjectResult(new
        {
            code = (int)HttpStatusCode.InternalServerError,
            msg = "服务器内部错误，请联系管理员",
            data = (object?)null
        })
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
}