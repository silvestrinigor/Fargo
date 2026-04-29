namespace Fargo.Domain.Events;

// TODO: Move to a new project Fargo.Types to remove the duplicated code in the sdk.
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

    /// <summary>
    /// Represents an API client entity.
    /// </summary>
    ApiClient = 5,
}
