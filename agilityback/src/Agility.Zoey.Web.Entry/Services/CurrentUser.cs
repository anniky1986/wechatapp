using System.Security.Claims;
using System.Text.Json;
using Agility.Zoey.Web.Core.Shared.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Agility.Zoey.Web.Entry.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(claim, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }

    public string UserName =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public long TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
            if (long.TryParse(claim, out var tenantId))
            {
                return tenantId;
            }
            return 0;
        }
    }

    public List<long> DataScopes
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("DataScopes", out var cached) == true
                && cached is List<long> cachedList)
            {
                return cachedList;
            }

            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("DataScopes")?.Value;
            if (!string.IsNullOrEmpty(claim))
            {
                try
                {
                    return JsonSerializer.Deserialize<List<long>>(claim) ?? new List<long>();
                }
                catch
                {
                    return claim.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => long.TryParse(s, out var n) ? n : 0)
                        .Where(n => n > 0)
                        .ToList();
                }
            }

            return new List<long>();
        }
    }

    public List<string> Permissions
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("Permission")?.Value;
            if (!string.IsNullOrEmpty(claim))
            {
                try
                {
                    return JsonSerializer.Deserialize<List<string>>(claim) ?? new List<string>();
                }
                catch
                {
                    return claim.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }

            return new List<string>();
        }
    }
}