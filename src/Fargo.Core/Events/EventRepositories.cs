namespace Fargo.Core.Events;

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

/// <summary>
/// Defines the repository contract for managing <see cref="EntityPartitionEvent"/> ledger entries.
/// </summary>
public interface IEntityPartitionEventRepository
{
    /// <summary>
    /// Adds a new entity partition event ledger entry to the persistence context.
    /// </summary>
    void Add(EntityPartitionEvent entityPartitionEvent);
}
