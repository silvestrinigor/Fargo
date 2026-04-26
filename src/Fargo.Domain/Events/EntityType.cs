namespace Fargo.Domain.Events;

/// <summary>
/// Represents the type of entity involved in an event.
/// Stored as an integer in the database.
/// Kept as a separate field from <see cref="EventType"/> to allow
/// efficient filtering by entity type without matching multiple event values.
/// </summary>
public enum EntityType
{
    Article = 0,
    Item = 1,
    User = 2,
    UserGroup = 3,
    Partition = 4,
    ApiClient = 5,
}
