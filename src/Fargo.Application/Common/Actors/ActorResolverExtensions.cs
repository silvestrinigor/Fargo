using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Common.Actors;

public static class ActorResolverExtensions
{
    public static async Task<SecurityPrincipal> ResolveCurrentActorOrThrowAsync(
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
