using Fargo.Core.Shared.Actors;

namespace Fargo.Application.Identity;

public interface ICurrentActor
{
    Guid Guid { get; }

    ActorType ActorType { get; }

    bool IsAuthenticated { get; }
}
