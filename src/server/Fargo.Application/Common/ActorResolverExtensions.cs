using Fargo.Core.Actors;

namespace Fargo.Application.Common;

/// <summary>
/// Application-layer convenience helpers over <see cref="ActorResolver"/>.
/// </summary>
public static class ActorResolverExtensions
{
    /// <summary>
    /// Resolves the actor for <paramref name="currentActor"/> and throws when it
    /// cannot be found. Centralizes the resolve-then-null-check pair that every
    /// command and query handler performs for the current actor.
    /// </summary>
    /// <param name="resolver">The actor resolver.</param>
    /// <param name="currentActor">The current authenticated actor context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved, non-null <see cref="Actor"/>.</returns>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when no active actor matches the current actor context.
    /// </exception>
    public static async Task<Actor> ResolveCurrentActorOrThrowAsync(
        this ActorResolver resolver,
        ICurrentActor currentActor,
        CancellationToken cancellationToken = default)
    {
        var actor = await resolver.GetActorByGuidAndTypeAsync(
            currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        return actor;
    }
}
