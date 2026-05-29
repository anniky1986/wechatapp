using System.Security.Claims;
using System.Text.Json;
using Agility.Zoey.Data.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Agility.Zoey.Web.Entry.Services;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(claim, out var userId))
        {
            return userId;
        }
        return null;
    }

    public long? GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
        if (long.TryParse(claim, out var tenantId))
        {
            return tenantId;
        }
        return null;
    }

    public List<long> GetCurrentUserDataScopes()
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
                var scopes = JsonSerializer.Deserialize<List<long>>(claim);
                return scopes ?? new List<long>();
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