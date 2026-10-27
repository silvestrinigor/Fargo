namespace Fargo.Core.Items.Events;

/// <summary>
/// Provides persistence operations for item events.
/// </summary>
public interface IItemEventRepository
{
    /// <summary>
    /// Adds an item event (with its payload) to be persisted.
    /// </summary>
    /// <param name="itemEvent">The item event to add.</param>
    void Add(ItemEvent itemEvent);
}
