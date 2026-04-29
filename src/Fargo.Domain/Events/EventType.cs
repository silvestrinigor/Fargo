namespace Fargo.Domain.Events;

// TODO: Move to a new project Fargo.Types to remove the duplicated code in the sdk.
/// <summary>
/// Represents the type of event that occurred on an entity.
/// Stored as an integer in the database.
/// </summary>
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

    /// <summary>
    /// Indicates that an API client was created.
    /// </summary>
    ApiClientCreated = 15,

    /// <summary>
    /// Indicates that an API client was updated.
    /// </summary>
    ApiClientUpdated = 16,

    /// <summary>
    /// Indicates that an API client was deleted.
    /// </summary>
    ApiClientDeleted = 17,
}
