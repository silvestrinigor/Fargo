namespace Fargo.ClientRealtime;

/// <summary>
/// Remembers the last N event identifiers, to drop at-least-once duplicates.
/// </summary>
/// <param name="capacity">How many identifiers to remember; 0 remembers none.</param>
internal sealed class RecentEventIds(int capacity)
{
    private readonly Queue<Guid> order = new();
    private readonly HashSet<Guid> seen = [];
    private readonly Lock gate = new();

    /// <summary>
    /// Records an identifier.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when it was not among the remembered identifiers.
    /// </returns>
    public bool TryAdd(Guid eventId)
    {
        if (capacity <= 0)
        {
            return true;
        }

        lock (gate)
        {
            if (!seen.Add(eventId))
            {
                return false;
            }

            order.Enqueue(eventId);

            if (order.Count > capacity)
            {
                seen.Remove(order.Dequeue());
            }

            return true;
        }
    }
}
