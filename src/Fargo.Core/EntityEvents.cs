using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;

namespace Fargo.Core;

/// <summary>
/// Represents the type of entity recorded in an entity event.
/// </summary>
public enum EntityType
{
    Article = 0,
    Item = 1,
    User = 2,
    UserGroup = 3,
    Partition = 4,
}

/// <summary>
/// Represents a simple lifecycle event that happened to an entity.
/// </summary>
public enum EntityEventType
{
    Created = 0,
    Deleted = 1,
    Activated = 2,
    Deactivated = 3,
    Moved = 4,
}

/// <summary>
/// Represents an append-only lifecycle event for an entity.
/// </summary>
public sealed class EntityEvent : Entity
{
    /// <summary>
    /// Initializes a new entity event.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private EntityEvent()
    {
    }

    private EntityEvent(
        EntityType entityType,
        EntityEventType eventType,
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
    public static EntityEvent EntityCreated<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity
        => Create(entity, EntityEventType.Created, actorGuid, occurredAt);

    /// <summary>
    /// Creates an entity deletion event for the specified entity.
    /// </summary>
    public static EntityEvent EntityDeleted<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity
        => Create(entity, EntityEventType.Deleted, actorGuid, occurredAt);

    /// <summary>
    /// Creates an entity activation event for the specified entity.
    /// </summary>
    public static EntityEvent Activated<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity
        => Create(entity, EntityEventType.Activated, actorGuid, occurredAt);

    /// <summary>
    /// Creates an entity deactivation event for the specified entity.
    /// </summary>
    public static EntityEvent Deactivated<TEntity>(
        TEntity entity,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        where TEntity : IEntity
        => Create(entity, EntityEventType.Deactivated, actorGuid, occurredAt);

    internal static EntityEvent ItemMoved(
        Item item,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        => Create(item, EntityEventType.Moved, actorGuid, occurredAt);

    private static EntityEvent Create<TEntity>(
        TEntity entity,
        EntityEventType eventType,
        Guid actorGuid,
        DateTimeOffset? occurredAt)
        where TEntity : IEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new EntityEvent(
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
    public EntityEventType EventType { get; private init; }

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
/// Defines the repository contract for managing <see cref="EntityEvent"/> ledger entries.
/// </summary>
public interface IEntityEventRepository
{
    /// <summary>
    /// Adds a new entity event ledger entry to the persistence context.
    /// </summary>
    void Add(EntityEvent entityEvent);
}
