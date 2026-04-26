namespace Fargo.Domain.Events;

/// <summary>
/// Represents the type of event that occurred on an entity.
/// Stored as an integer in the database.
/// </summary>
public enum EventType
{
    ArticleCreated = 0,
    ArticleUpdated = 1,
    ArticleDeleted = 2,

    ItemCreated = 3,
    ItemUpdated = 4,
    ItemDeleted = 5,

    UserCreated = 6,
    UserUpdated = 7,
    UserDeleted = 8,

    UserGroupCreated = 9,
    UserGroupUpdated = 10,
    UserGroupDeleted = 11,

    PartitionCreated = 12,
    PartitionUpdated = 13,
    PartitionDeleted = 14,

    ApiClientCreated = 15,
    ApiClientUpdated = 16,
    ApiClientDeleted = 17,
}
