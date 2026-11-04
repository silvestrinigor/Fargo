using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Common;

public interface ICurrentActor
{
    Guid Guid { get; }

    ActorType ActorType { get; }

    bool IsAuthenticated { get; }
}
