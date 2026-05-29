namespace Agility.Zoey.Web.Core.Shared.Consts;

public static class SystemConsts
{
    public const string SuperAdminRoleCode = "super_admin";
    public const string NormalUserRoleCode = "normal_user";
    public const string DefaultTenantCode = "default";
    public const string DefaultPassword = "Admin@123";
    public const int MaxLoginFailCount = 5;
    public const int LockDurationMinutes = 30;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const long MaxFileSize = 10 * 1024 * 1024;
    public const string UploadsFolder = "uploads";
    public const string DefaultAvatar = "/avatar/default.png";
}