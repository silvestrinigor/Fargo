using Fargo.Core.Actors;

namespace Fargo.Application.Common;

public interface ICurrentActor
{
    Guid Guid { get; }

    ActorType ActorType { get; }

    bool IsAuthenticated { get; }
}
