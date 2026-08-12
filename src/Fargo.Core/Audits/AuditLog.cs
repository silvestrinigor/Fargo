using Fargo.Core.Actors;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Entities;

namespace Fargo.Core.Audits;

public class AuditLog : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
{
    public Guid Guid { get; private set; } = Guid.NewGuid();

    public Guid ActorGuid { get; private init; }

    public ActorType ActorType { get; private init; }

    public ActionType ActionType { get; private init; }

    public Guid EntityGuid { get; private init; }

    public EntityType EntityType { get; private init; }

    public DateTimeOffset OccurredAt { get; private init; } = DateTimeOffset.UtcNow;

    public AuditMetadata Metadata { get; private init; } = new();

    public IReadOnlyCollection<AuditLogPartition> Partitions => partitions;

    private readonly List<AuditLogPartition> partitions = [];

    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private AuditLog(Guid actorGuid, ActorType actorType, Guid entityGuid, EntityType entityType, ActionType actionType, IReadOnlyCollection<Guid> partitions)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        EntityGuid = entityGuid;
        EntityType = entityType;
        ActionType = actionType;

        foreach (var p in partitions)
        {
            AddPartition(p);
        }
    }

    public static AuditLog CreateAuditLog<TEntity>(Actor actor, TEntity entity, ActionType actionType)
        where TEntity : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
    {
        return new AuditLog(actor.Guid, actor.ActorType, entity.Guid, entity.GetEntityType(), actionType, entity.PartitionGuids);
    }

    public static AuditLog CreateAuditLog(Actor actor, Partition partition, ActionType actionType)
    {
        return new AuditLog(
            actor.Guid, actor.ActorType, partition.Guid, partition.GetEntityType(), actionType,
            [partition.ParentPartitionGuid ?? FargoCoreWellKnowGuids.GlobalPartitionGuid]);
    }

    private void AddPartition(Guid partitionGuid)
    {
        if (partitions.Any(p => p.PartitionGuid == partitionGuid))
        {
            return;
        }

        partitions.Add(new AuditLogPartition(this, partitionGuid));
    }

    public EntityType GetEntityType() => EntityType.AuditLog;
}
