using Fargo.Core.Actors;
using Fargo.Core.Entities;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Entities;

namespace Fargo.Core.Audits;

public class AuditLog : IEntity
{
    public Guid Guid { get; private set; } = Guid.NewGuid();

    public Guid ActorGuid { get; private init; }

    public ActorType ActorType { get; private init; }

    public ActionType ActionType { get; private init; }

    public Guid EntityGuid { get; private init; }

    public EntityType EntityType { get; private init; }

    public DateTimeOffset OccurredAt { get; private init; } = DateTimeOffset.UtcNow;

    public AuditMetadata Metadata { get; private init; } = new();

    private AuditLog() { }

    public static AuditLog CreateAuditLog(
        Actor actor, Guid entityGuid, EntityType entityType, ActionType actionType)
    {
        return new AuditLog()
        {
            ActorGuid = actor.Guid,
            ActorType = actor.ActorType,
            EntityGuid = entityGuid,
            EntityType = entityType,
            ActionType = actionType
        };
    }
}
