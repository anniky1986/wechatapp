namespace Agility.Zoey.Data.Interfaces;

public interface ICurrentUserAccessor
{
    long? GetCurrentUserId();

    long? GetCurrentTenantId();

    List<long> GetCurrentUserDataScopes();
}