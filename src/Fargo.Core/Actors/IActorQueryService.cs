using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public interface IActorQueryService
{
    Task<Actor?> GetActorByActorIdAsync(ActorId actorId, CancellationToken cancellationToken = default);
}
