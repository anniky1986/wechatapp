using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agility.Zoey.Web.Core.Middleware;

public class RequestLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLogMiddleware> _logger;

    public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var requestBody = string.Empty;

        if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        await _next(context);

        sw.Stop();

        var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel,
            "Request {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. Body: {RequestBody}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            requestBody.Length > 1000 ? requestBody[..1000] + "..." : requestBody);
    }
}