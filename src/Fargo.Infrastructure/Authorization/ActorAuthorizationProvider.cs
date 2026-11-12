using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Infrastructure.Caching;

namespace Fargo.Infrastructure.Authorization;

/// <summary>
/// Checks the actor uncached on every call, then serves its permissions and partition accesses from
/// the <see cref="IActorAuthorizationCache"/>, keyed by the authorization stamp, or from the database
/// after something that affects authorization changed.
/// </summary>
public sealed class ActorAuthorizationProvider(
    ActorAuthorizationStateReader stateReader,
    IActorAuthorizationCache cache,
    IUserRepository userRepository,
    IServiceAccountRepository serviceAccountRepository) : IActorAuthorizationProvider
{
    public async Task<ActorAuthorization?> GetAsync(
        Guid actorGuid,
        ActorType actorType,
        CancellationToken cancellationToken = default)
    {
        var state = await stateReader.GetAsync(actorGuid, actorType, cancellationToken);

        if (state is not { IsActive: true })
        {
            return null;
        }

        // Without a stamp a cached entry could never be invalidated, so read directly.
        var cached = state.AuthorizationStamp == Guid.Empty
            ? await LoadAsync(actorType, actorGuid, cancellationToken)
            : await cache.GetOrCreateAsync(
                new ActorAuthorizationCacheKey(actorType, actorGuid, state.AuthorizationStamp),
                ct => LoadAsync(actorType, actorGuid, ct),
                cancellationToken);

        return new ActorAuthorization(cached.Permissions, cached.PartitionAccessGuids);
    }

    private async Task<CachedActorAuthorization> LoadAsync(
        ActorType actorType,
        Guid actorGuid,
        CancellationToken cancellationToken)
    {
        if (actorType == ActorType.User)
        {
            var permissions = await userRepository.GetAllActivePermissionsFromUserAsync(actorGuid, cancellationToken);
            var partitionAccesses = await userRepository.GetAllActivePartitionAccessGuidsFromUserAsync(actorGuid, cancellationToken);

            return new CachedActorAuthorization([.. permissions], [.. partitionAccesses]);
        }

        // Service accounts have no groups: their permissions are their own. Accesses are expanded
        // down the partition tree, as for users, so both kinds authorize alike.
        var service = await serviceAccountRepository.GetByGuidAsync(actorGuid, cancellationToken);

        if (service is null)
        {
            return new CachedActorAuthorization([], []);
        }

        var serviceAccesses = await serviceAccountRepository.GetAllActivePartitionAccessGuidsAsync(actorGuid, cancellationToken);

        return new CachedActorAuthorization([.. service.Actor.Permissions], [.. serviceAccesses]);
    }
}
