namespace Fargo.Core.Entities.Actors;

/// <summary>
/// A permission granted directly to an actor.
/// </summary>
public class ActorPermission
{
    public Guid ActorGuid { get; private init; }

    public Actor Actor { get; private init; }

    public ActionType ActionType { get; private init; }

    // Required by EF Core.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private ActorPermission()
#pragma warning restore CS8618
    {
    }

    internal ActorPermission(Actor actor, ActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Actor = actor;
        ActorGuid = actor.Guid;
        ActionType = actionType;
    }
}