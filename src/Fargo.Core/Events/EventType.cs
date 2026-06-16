namespace Fargo.Core.Events;

/// <summary>
/// Represents a simple lifecycle event that happened to an entity.
/// </summary>
public enum EventType
{
    EntityCreated = 0,
    EntityDeleted = 1,
    EntityActivated = 2,
    EntityDeactivated = 3,
    ItemMoved = 4,
    InsertedIntoPartition = 5,
    RemovedFromPartition = 6,
}