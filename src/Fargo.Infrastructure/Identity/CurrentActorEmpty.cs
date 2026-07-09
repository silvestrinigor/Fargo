using Fargo.Core.Shared.Actors;

namespace Fargo.Application.Identity;

public sealed class CurrentActorEmpty : ICurrentActor
{
    public ActorId ActorId => ActorId.Empty;

    public bool IsAuthenticated => false;
}
