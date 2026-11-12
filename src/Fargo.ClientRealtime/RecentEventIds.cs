namespace Fargo.ClientRealtime;

/// <summary>
/// Remembers the last N event identifiers, to drop at-least-once duplicates.
/// </summary>
internal sealed class RecentEventIds(int capacity)
{
    private readonly Queue<Guid> order = new();
    private readonly HashSet<Guid> seen = [];
    private readonly Lock gate = new();

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
