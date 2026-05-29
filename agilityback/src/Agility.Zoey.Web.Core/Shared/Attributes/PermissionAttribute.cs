namespace Agility.Zoey.Web.Core.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : Attribute
{
    public string PermissionCode { get; }

    public PermissionAttribute(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}