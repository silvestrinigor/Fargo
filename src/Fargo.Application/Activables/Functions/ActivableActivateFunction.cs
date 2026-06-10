using Fargo.Core.Activables;
using Fargo.Core.Actors;
using Fargo.Core.Events;

namespace Fargo.Application.Activables.Functions;

internal static class ActivableActivateFunction
{
    internal static void ActivateEntity(IActivable activable, Actor actor, IEventRepository eventRepository)
    {
        activable.Activate(actor);

        var activated = Event.Activated(activable, actor.Guid);

        eventRepository.Add(activated);
    }
}
