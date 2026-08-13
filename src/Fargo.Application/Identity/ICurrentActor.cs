using Fargo.Core.Actors;

namespace Fargo.Application.Identity;

public interface ICurrentActor
{
    Guid Guid { get; }

    ActorType ActorType { get; }

    bool IsAuthenticated { get; }
}
