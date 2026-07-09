using Fargo.Core.Shared.Actors;
using Fargo.Core.System;

namespace Fargo.Application.Identity;

public sealed class CurrentActorSystem : ICurrentActor
{
    public ActorId ActorId => new(SystemService.SystemGuid, ActorType.Application);

    public bool IsAuthenticated => true;
}
