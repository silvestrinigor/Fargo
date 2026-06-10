using Fargo.Core.Activables;
using Fargo.Core.Actors;
using Fargo.Core.Events;

namespace Fargo.Application.Activables.Functions;

internal static class ActivableDeactivateFunction
{
    internal static void DeactivateEntity(IActivable activable, Actor actor, IEventRepository eventRepository)
    {
        activable.Deactivate(actor);

        var deactivated = Event.Deactivated(activable, actor.Guid);

        eventRepository.Add(deactivated);
    }
}
