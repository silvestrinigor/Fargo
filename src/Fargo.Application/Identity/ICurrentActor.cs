using Fargo.Core.Shared.Actors;

namespace Fargo.Application.Identity;

public interface ICurrentActor
{
    ActorId ActorId { get; }

    bool IsAuthenticated { get; }
}
