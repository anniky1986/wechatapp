namespace Agility.Zoey.Web.Core.Shared.Consts;

public static class CacheKeys
{
    public const string UserPermissions = "user:permissions:{0}";
    public const string UserInfo = "user:info:{0}";
    public const string UserRoles = "user:roles:{0}";
    public const string RolePermissions = "role:permissions:{0}";
    public const string MenuTree = "menu:tree";
    public const string MenuRoutes = "menu:routes";
    public const string DictItems = "dict:items:{0}";
    public const string SystemSettings = "system:settings";
    public const string SystemSettingGroup = "system:settings:{0}";
    public const string DeptTree = "dept:tree";
    public const string TenantInfo = "tenant:info:{0}";
    public const string OnlineUsers = "online:users";
    public const string BlacklistedTokens = "auth:blacklist";
    public const string RefreshTokens = "auth:refresh:{0}";
}