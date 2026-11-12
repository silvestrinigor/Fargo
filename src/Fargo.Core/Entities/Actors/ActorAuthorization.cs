namespace Fargo.Core.Entities.Actors;

/// <summary>
/// An actor's effective permissions, including those inherited from user groups, and its partition
/// accesses expanded down the partition tree.
/// </summary>
public sealed record ActorAuthorization(
    IReadOnlyList<ActionType> Permissions,
    IReadOnlyList<Guid> PartitionAccessGuids);

public interface IActorAuthorizationProvider
{
    /// <summary>
    /// Returns <see langword="null"/> when no actor of that type exists or the actor is inactive.
    /// Deactivation must take effect immediately.
    /// </summary>
    Task<ActorAuthorization?> GetAsync(Guid actorGuid, ActorType actorType, CancellationToken cancellationToken = default);
}
