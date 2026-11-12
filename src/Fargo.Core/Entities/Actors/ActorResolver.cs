using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Core.Entities.Actors;

/// <summary>
/// Resolves the <see cref="SecurityPrincipal"/> (authorization context) of an actor.
/// </summary>
public sealed class ActorResolver(IUserRepository userRepository, IServiceAccountRepository serviceAccountRepository)
{
    /// <summary>
    /// Returns <see langword="null"/> when no actor of that type exists or the actor is inactive.
    /// </summary>
    public async Task<SecurityPrincipal?> GetActorByGuidAndTypeAsync(Guid actorGuid, ActorType actorType, CancellationToken cancellationToken = default)
    {
        if (actorType == ActorType.User)
        {
            return await GetUserActorByGuidAsync(actorGuid, cancellationToken);
        }

        if (actorType == ActorType.Service)
        {
            return await GetServiceActorByGuidAsync(actorGuid, cancellationToken);
        }

        return null;
    }

    private async Task<SecurityPrincipal?> GetServiceActorByGuidAsync(Guid serviceGuid, CancellationToken cancellationToken = default)
    {
        var service = await serviceAccountRepository.GetByGuidAsync(serviceGuid, cancellationToken);

        if (service is null || !service.Actor.IsActive)
        {
            return null;
        }

        // Accesses are expanded down the partition tree, as for users, so both kinds authorize alike.
        var partitionAccessGuids = await serviceAccountRepository
            .GetAllActivePartitionAccessGuidsAsync(service.ActorGuid, cancellationToken);

        var actor = new SecurityPrincipal(
            service.ActorGuid,
            ActorType.Service,
            service.Actor.Permissions.ToHashSet(),
            partitionAccessGuids.ToHashSet());

        return actor;
    }

    private async Task<SecurityPrincipal?> GetUserActorByGuidAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuidAsync(userGuid, cancellationToken);

        if (user is null || !user.Actor.IsActive)
        {
            return null;
        }

        var partitionAccessGuids = await userRepository.GetAllActivePartitionAccessGuidsFromUserAsync(user.ActorGuid, cancellationToken);

        var permissions = await userRepository.GetAllActivePermissionsFromUserAsync(user.ActorGuid, cancellationToken);

        var actor = new SecurityPrincipal(user.ActorGuid, ActorType.User, permissions.ToHashSet(), partitionAccessGuids.ToHashSet());

        return actor;
    }
}
