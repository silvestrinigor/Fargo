using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public interface IActorQueryService
{
    Task<Actor?> GetActorByActorId(ActorId actorId, CancellationToken cancellationToken = default);
}
