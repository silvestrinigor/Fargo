

namespace Fargo.Core.Events;

#region Types

/// <summary>
/// Represents the type of entity involved in an event.
/// Stored as an integer in the database.
/// Kept as a separate field from <see cref="EventType"/> to allow
/// efficient filtering by entity type without matching multiple event values.
/// </summary>
public enum EntityType
{
    /// <summary>
    /// Represents an article entity.
    /// </summary>
    Article = 0,

    /// <summary>
    /// Represents an item entity.
    /// </summary>
    Item = 1,

    /// <summary>
    /// Represents a user entity.
    /// </summary>
    User = 2,

    /// <summary>
    /// Represents a user group entity.
    /// </summary>
    UserGroup = 3,

    /// <summary>
    /// Represents a partition entity.
    /// </summary>
    Partition = 4,

}
/// <summary>
/// Defines the type of event that occurred on a domain entity.
/// </summary>
/// <remarks>
/// This enum is stored as an integer in the database to ensure
/// compact storage and fast comparisons.
/// 
/// Each value represents a specific action (Created, Updated, Deleted)
/// applied to a specific entity type.
/// 
/// This enum is typically used alongside <see cref="EntityType"/> to:
/// - Fully describe an event in the system
/// - Enable fine-grained filtering and querying of events
/// - Support event sourcing or audit logging scenarios
/// 
/// Naming convention:
/// {Entity}{Action}
/// </remarks>
public enum EventType
{
    /// <summary>
    /// Indicates that an article was created.
    /// </summary>
    ArticleCreated = 0,

    /// <summary>
    /// Indicates that an article was updated.
    /// </summary>
    ArticleUpdated = 1,

    /// <summary>
    /// Indicates that an article was deleted.
    /// </summary>
    ArticleDeleted = 2,

    /// <summary>
    /// Indicates that an item was created.
    /// </summary>
    ItemCreated = 3,

    /// <summary>
    /// Indicates that an item was updated.
    /// </summary>
    ItemUpdated = 4,

    /// <summary>
    /// Indicates that an item was deleted.
    /// </summary>
    ItemDeleted = 5,

    /// <summary>
    /// Indicates that a user was created.
    /// </summary>
    UserCreated = 6,

    /// <summary>
    /// Indicates that a user was updated.
    /// </summary>
    UserUpdated = 7,

    /// <summary>
    /// Indicates that a user was deleted.
    /// </summary>
    UserDeleted = 8,

    /// <summary>
    /// Indicates that a user group was created.
    /// </summary>
    UserGroupCreated = 9,

    /// <summary>
    /// Indicates that a user group was updated.
    /// </summary>
    UserGroupUpdated = 10,

    /// <summary>
    /// Indicates that a user group was deleted.
    /// </summary>
    UserGroupDeleted = 11,

    /// <summary>
    /// Indicates that a partition was created.
    /// </summary>
    PartitionCreated = 12,

    /// <summary>
    /// Indicates that a partition was updated.
    /// </summary>
    PartitionUpdated = 13,

    /// <summary>
    /// Indicates that a partition was deleted.
    /// </summary>
    PartitionDeleted = 14,

}

#endregion Types

#region Entity

/// <summary>
/// Represents a domain event that occurred on an entity.
/// Events are append-only and immutable after creation.
/// </summary>
public sealed class Event : Entity
{
    /// <summary>Gets the type of event that occurred.</summary>
    public EventType EventType { get; init; }

    /// <summary>Gets the unique identifier of the entity involved in the event.</summary>
    public Guid EntityGuid { get; init; }

    /// <summary>Gets the type of the entity involved in the event.</summary>
    public EntityType EntityType { get; init; }

    /// <summary>Gets the point in time when the event occurred.</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the unique identifier of the user who triggered the event.</summary>
    public Guid ActorGuid { get; init; }
}

#endregion Entity
