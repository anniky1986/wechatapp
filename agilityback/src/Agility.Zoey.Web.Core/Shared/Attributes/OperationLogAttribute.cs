namespace Agility.Zoey.Web.Core.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class OperationLogAttribute : Attribute
{
    public string Module { get; }

    public string Description { get; }

    public OperationLogAttribute(string module, string description)
    {
        Module = module;
        Description = description;
    }
}