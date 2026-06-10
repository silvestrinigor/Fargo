namespace Fargo.Core.Events;

/// <summary>
/// Defines the repository contract for managing <see cref="Event"/> ledger entries.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Adds a new entity event ledger entry to the persistence context.
    /// </summary>
    void Add(Event entityEvent);
}

/// <summary>
/// Defines the repository contract for managing <see cref="PartitionEvent"/> ledger entries.
/// </summary>
public interface IPartitionEventRepository
{
    /// <summary>
    /// Adds a new entity partition event ledger entry to the persistence context.
    /// </summary>
    void Add(PartitionEvent entityPartitionEvent);
}
