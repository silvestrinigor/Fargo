using Fargo.Core.Activables;
using Fargo.Core.Actors;
using Fargo.Core.Events;

namespace Fargo.Application.Activables.Functions;

internal static class ActivableActivateFunction
{
    internal static void ActivateEntity(IActivable activable, bool isActive, Actor actor, IEventRepository eventRepository)
    {
        activable.IsActive = isActive;

        var activated = Event.Activated(activable, actor.ActorId);

        eventRepository.Add(activated);
    }
}
