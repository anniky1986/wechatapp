using System.Net;
using Agility.Zoey.Web.Core.Shared.Attributes;
using Agility.Zoey.Web.Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Agility.Zoey.Web.Core.Filters;

public class PermissionFilter : IAsyncActionFilter
{
    private readonly ICurrentUser _currentUser;

    public PermissionFilter(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var permissionAttributes = context.ActionDescriptor.EndpointMetadata
            .OfType<PermissionAttribute>()
            .ToList();

        if (permissionAttributes.Count == 0)
        {
            await next();
            return;
        }

        var userPermissions = _currentUser?.Permissions ?? new List<string>();

        var hasPermission = permissionAttributes.Any(attr =>
            userPermissions.Contains(attr.PermissionCode, StringComparer.OrdinalIgnoreCase));

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new
            {
                code = (int)HttpStatusCode.Forbidden,
                msg = "无权限访问",
                data = (object?)null
            })
            {
                StatusCode = (int)HttpStatusCode.OK
            };
            return;
        }

        await next();
    }
}