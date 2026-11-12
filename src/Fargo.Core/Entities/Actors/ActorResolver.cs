namespace Fargo.Core.Entities.Actors;

/// <summary>
/// Resolves the <see cref="SecurityPrincipal"/> (authorization context) of an actor.
/// </summary>
public sealed class ActorResolver(IActorAuthorizationProvider authorizationProvider)
{
    /// <summary>
    /// Returns <see langword="null"/> when no actor of that type exists or the actor is inactive.
    /// </summary>
    public async Task<SecurityPrincipal?> GetActorByGuidAndTypeAsync(Guid actorGuid, ActorType actorType, CancellationToken cancellationToken = default)
    {
        var authorization = await authorizationProvider.GetAsync(actorGuid, actorType, cancellationToken);

        if (authorization is null)
        {
            return null;
        }

        return new SecurityPrincipal(
            actorGuid,
            actorType,
            authorization.Permissions.ToHashSet(),
            authorization.PartitionAccessGuids.ToHashSet());
    }
}
