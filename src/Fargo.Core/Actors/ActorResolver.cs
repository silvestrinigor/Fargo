using Fargo.Core.Shared.Actors;
using Fargo.Core.Users;

namespace Fargo.Core.Actors;

/// <summary>
/// Provides services for retrieving actors and their authorization context.
/// </summary>
/// <param name="userRepository">
/// The repository used to retrieve users and their permissions and partition accesses.
/// </param>
public sealed class ActorResolver(IUserRepository userRepository)
{
    /// <summary>
    /// Gets an actor by its unique identifier and actor type.
    /// </summary>
    /// <param name="actorGuid">
    /// The unique identifier of the actor.
    /// </param>
    /// <param name="actorType">
    /// The type of actor to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The actor if an active actor of the specified type exists;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<Actor?> GetActorByGuidAndTypeAsync(Guid actorGuid, ActorType actorType, CancellationToken cancellationToken = default)
    {
        if (actorType == ActorType.User)
        {
            return await GetUserActorByGuidAsync(actorGuid, cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets an actor representing an active user and its authorization context.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// An actor containing the user's permissions and partition accesses if
    /// the user exists and is active; otherwise, <see langword="null"/>.
    /// </returns>
    private async Task<Actor?> GetUserActorByGuidAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuidAsync(userGuid, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var partitionAccessGuids = await userRepository.GetAllActivePartitionAccessGuidsFromUserAsync(user.Guid, cancellationToken);

        var permissions = await userRepository.GetAllActivePermissionsFromUserAsync(user.Guid, cancellationToken);

        var actor = new Actor(user.Guid, ActorType.User, permissions.ToHashSet(), partitionAccessGuids.ToHashSet());

        return actor;
    }
}
