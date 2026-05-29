namespace Agility.Zoey.Web.Core.Shared.Interfaces;

public interface ICurrentUser
{
    long UserId { get; }
    string UserName { get; }
    long TenantId { get; }
    List<long> DataScopes { get; }
    List<string> Permissions { get; }
}