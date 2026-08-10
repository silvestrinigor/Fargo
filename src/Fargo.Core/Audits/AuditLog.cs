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

    public DateTimeOffset OccurredAt { get; private init; }

    public IReadOnlyDictionary<string, object?> Metadata => metadata;

    private readonly Dictionary<string, object?> metadata = new(StringComparer.Ordinal);

    private AuditLog() { }

    private AuditLog(Dictionary<string, object?>? metadata)
    {
        if (metadata is not null)
        {
            this.metadata = metadata;
        }
    }

    public static AuditLog CreateAuditLog(
        Actor actor, Guid entityGuid, EntityType entityType, ActionType actionType, Dictionary<string, object?>? metadata = null)
    {
        return new AuditLog(metadata)
        {
            ActorGuid = actor.Guid,
            ActorType = actor.ActorType,
            EntityGuid = entityGuid,
            EntityType = entityType,
            ActionType = actionType
        };
    }
}
