using Fargo.Core.Actors;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Entities;

namespace Fargo.Core.Audits;

/// <summary>
/// Represents an immutable record of an action performed by an actor on an entity.
/// </summary>
/// <remarks>
/// Audit logs are partition-controlled in the same way as other partitioned
/// entities. The difference is that their partition access is derived from the
/// entity being audited rather than being assigned independently.
///
/// An audit log is associated with every partition that provides access to the
/// audited entity at the time the log is created. Consequently, access to an
/// audit log follows the access scope of the entity whose action it records.
///
/// For partition actions, the audit log is associated with the affected
/// partition's parent partition, or with the global partition when the affected
/// partition has no parent.
/// </remarks>
public class AuditLog : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
{
    /// <summary>
    /// Gets the unique identifier of the audit log.
    /// </summary>
    public Guid Guid { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the identifier of the actor that performed the action.
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the type of actor that performed the action.
    /// </summary>
    public ActorType ActorType { get; private init; }

    /// <summary>
    /// Gets the type of action recorded by this audit log.
    /// </summary>
    public ActionType ActionType { get; private init; }

    /// <summary>
    /// Gets the identifier of the entity affected by the action.
    /// </summary>
    public Guid EntityGuid { get; private init; }

    /// <summary>
    /// Gets the type of entity affected by the action.
    /// </summary>
    public EntityType EntityType { get; private init; }

    /// <summary>
    /// Gets the date and time at which the action occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the metadata associated with the audited action.
    /// </summary>
    public AuditMetadata Metadata { get; private init; } = new();

    /// <summary>
    /// Gets the partitions through which this audit log is accessible.
    /// </summary>
    /// <remarks>
    /// These partitions are derived from the partitions associated with the
    /// audited entity when the audit log is created.
    /// </remarks>
    public IReadOnlyCollection<AuditLogPartition> Partitions => partitions;

    private readonly List<AuditLogPartition> partitions = [];

    /// <summary>
    /// Gets the identifiers of the partitions through which this audit log
    /// is accessible.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private AuditLog() { }

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

    /// <summary>
    /// Creates an audit log for an action performed on a partitioned entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the audited entity.</typeparam>
    /// <param name="actor">The actor that performed the action.</param>
    /// <param name="entity">The entity affected by the action.</param>
    /// <param name="actionType">The action that was performed.</param>
    /// <returns>A new audit log associated with the partitions of the entity.</returns>
    /// <remarks>
    /// The audit log inherits its partition access from the audited entity.
    /// Every partition associated with the entity at the time of the action
    /// is copied to the audit log.
    /// </remarks>
    public static AuditLog CreateAuditLog<TEntity>(Actor actor, TEntity entity, ActionType actionType)
        where TEntity : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
    {
        return new AuditLog(actor.Guid, actor.ActorType, entity.Guid, entity.GetEntityType(), actionType, entity.PartitionGuids);
    }

    /// <summary>
    /// Creates an audit log for an action performed on a partition.
    /// </summary>
    /// <param name="actor">The actor that performed the action.</param>
    /// <param name="partition">The partition affected by the action.</param>
    /// <param name="actionType">The action that was performed.</param>
    /// <returns>A new audit log associated with the appropriate parent partition.</returns>
    /// <remarks>
    /// A partition action is associated with its parent partition. If the
    /// affected partition has no parent, the audit log is associated with the
    /// global partition.
    /// </remarks>
    public static AuditLog CreateAuditLog(Actor actor, Partition partition, ActionType actionType)
    {
        return new AuditLog(
            actor.Guid, actor.ActorType, partition.Guid, partition.GetEntityType(), actionType,
            [partition.ParentPartitionGuid ?? FargoCoreWellKnowGuids.GlobalPartitionGuid]);
    }

    /// <summary>
    /// Adds a partition through which the audit log can be accessed.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition.</param>
    /// <remarks>
    /// Duplicate partition associations are ignored.
    /// </remarks>
    private void AddPartition(Guid partitionGuid)
    {
        if (partitions.Any(p => p.PartitionGuid == partitionGuid))
        {
            return;
        }

        partitions.Add(new AuditLogPartition(this, partitionGuid));
    }

    /// <inheritdoc/>
    public EntityType GetEntityType() => EntityType.AuditLog;
}
