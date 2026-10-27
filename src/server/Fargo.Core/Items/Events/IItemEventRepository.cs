namespace Fargo.Core.Items.Events;

/// <summary>
/// Provides persistence operations for item events.
/// </summary>
public interface IItemEventRepository
{
    /// <summary>
    /// Adds a movement event to be persisted.
    /// </summary>
    /// <param name="itemMovimentEvent">The movement event to add.</param>
    void Add(ItemMovimentEvent itemMovimentEvent);
}
