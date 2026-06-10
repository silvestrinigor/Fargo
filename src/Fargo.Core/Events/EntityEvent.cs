using Fargo.Core.Activables;
using Fargo.Core.Articles;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;

namespace Fargo.Core.Events;

/// <summary>
/// Represents a simple lifecycle event that happened to an entity.
/// </summary>
public enum EventType
{
    Created = 0,
    Deleted = 1,
    Activated = 2,
    Deactivated = 3,
    Moved = 4,
    InsertedIntoPartition = 5,
    RemovedFromPartition = 6,
}

/// <summary>
/// Represents event for an entity.
/// </summary>
public sealed class Event : Entity
{
    /// <summary>
    /// Initializes a new entity event.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private Event()
    {
    }

    private Event(
        EntityType entityType,
        EventType eventType,
        Guid entityGuid,
        Guid actorGuid,
        DateTimeOffset occurredAt)
    {
        if (entityGuid == Guid.Empty)
        {
            throw new ArgumentException("Entity event entity guid cannot be empty.", nameof(entityGuid));
        }

        if (actorGuid == Guid.Empty)
        {
            throw new ArgumentException("Entity event actor guid cannot be empty.", nameof(actorGuid));
        }

        EntityType = entityType;
        EventType = eventType;
        EntityGuid = entityGuid;
        ActorGuid = actorGuid;
        OccurredAt = occurredAt;
    }

    /// <summary>
    /// Creates an entity creation event for the specified entity.
    /// </summary>
    public static Event NewEntityCreatedEvent<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity
        => Create(entity, EventType.Created, actorGuid, occurredAt);

    /// <summary>
    /// Creates an entity deletion event for the specified entity.
    /// </summary>
    public static Event EntityDeleted<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity
        => Create(entity, EventType.Deleted, actorGuid, occurredAt);

    /// <summary>
    /// Creates an entity activation event for the specified entity.
    /// </summary>
    public static Event Activated<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity, IActivable
        => Create(entity, EventType.Activated, actorGuid, occurredAt);

    /// <summary>
    /// Creates an entity deactivation event for the specified entity.
    /// </summary>
    public static Event Deactivated<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity, IActivable
        => Create(entity, EventType.Deactivated, actorGuid, occurredAt);

    internal static Event ItemMoved(
        Item item,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        => Create(item, EventType.Moved, actorGuid, occurredAt);

    internal static Event InsertedIntoPartition<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity, IPartitionedEntity
        => Create(entity, EventType.InsertedIntoPartition, actorGuid, occurredAt);

    internal static Event RemovedFromPartition<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity, IPartitionedEntity
        => Create(entity, EventType.RemovedFromPartition, actorGuid, occurredAt);

    private static Event Create<TEntity>(
        TEntity entity,
        EventType eventType,
        Guid actorGuid,
        DateTimeOffset? occurredAt)
        where TEntity : IEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new Event(
            ToEntityType<TEntity>(),
            eventType,
            entity.Guid,
            actorGuid,
            occurredAt ?? DateTimeOffset.UtcNow);
    }

    private static EntityType ToEntityType<TEntity>()
        where TEntity : IEntity
        => typeof(TEntity) switch
        {
            var type when type == typeof(Article) => EntityType.Article,
            var type when type == typeof(Item) => EntityType.Item,
            var type when type == typeof(User) => EntityType.User,
            var type when type == typeof(UserGroup) => EntityType.UserGroup,
            var type when type == typeof(Partition) => EntityType.Partition,
            _ => throw new ArgumentException(
                $"Entity type '{typeof(TEntity).Name}' cannot be recorded as an entity event.",
                nameof(TEntity)),
        };

    /// <summary>
    /// Gets the type of entity that the event belongs to.
    /// </summary>
    public EntityType EntityType { get; private init; }

    /// <summary>
    /// Gets the lifecycle event type.
    /// </summary>
    public EventType EventType { get; private init; }

    /// <summary>
    /// Gets the entity unique identifier.
    /// </summary>
    public Guid EntityGuid { get; private init; }

    /// <summary>
    /// Gets the actor unique identifier that performed the event.
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private init; }
}

/// <summary>
/// Represents partition assignment details attached to an entity event.
/// </summary>
/// <remarks>
/// The related <see cref="Events.Event"/> stores the affected entity, actor, and occurrence time.
/// This row stores only the affected partition.
/// </remarks>
public sealed class PartitionEvent : Entity
{
    /// <summary>
    /// Initializes a new entity partition event.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private PartitionEvent()
    {
    }

    private PartitionEvent(
        Event entityEvent,
        Guid partitionGuid)
    {
        Event = entityEvent;
        Guid = entityEvent.Guid;
        PartitionGuid = partitionGuid;
    }

    /// <summary>
    /// Creates a partition insertion detail and its related entity event.
    /// </summary>
    public static PartitionEvent InsertedIntoPartition<TEntity>(
        TEntity entity,
        Partition partition,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity, IPartitionedEntity
    {
        ArgumentNullException.ThrowIfNull(partition);

        return new(
            Event.InsertedIntoPartition(entity, actorGuid, occurredAt),
            partition.Guid);
    }

    /// <summary>
    /// Creates a partition removal detail and its related entity event.
    /// </summary>
    public static PartitionEvent RemovedFromPartition<TEntity>(
        TEntity entity,
        Partition partition,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity, IPartitionedEntity
    {
        ArgumentNullException.ThrowIfNull(partition);

        return new(
            Event.RemovedFromPartition(entity, actorGuid, occurredAt),
            partition.Guid);
    }

    /// <summary>
    /// Gets the related entity event.
    /// </summary>
    public Event Event { get; private init; } = null!;

    /// <summary>
    /// Gets the affected entity type.
    /// </summary>
    public EntityType EntityType => Event.EntityType;

    /// <summary>
    /// Gets the affected entity unique identifier.
    /// </summary>
    public Guid EntityGuid => Event.EntityGuid;

    /// <summary>
    /// Gets the affected partition unique identifier.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the actor unique identifier that performed the partition assignment change.
    /// </summary>
    public Guid ActorGuid => Event.ActorGuid;

    /// <summary>
    /// Gets the date and time when the partition assignment changed.
    /// </summary>
    public DateTimeOffset OccurredAt => Event.OccurredAt;
}
