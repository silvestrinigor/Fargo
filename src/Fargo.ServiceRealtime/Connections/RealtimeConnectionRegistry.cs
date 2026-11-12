using System.Collections.Concurrent;

namespace Fargo.ServiceRealtime.Connections;

/// <summary>
/// The open hub connections of this instance.
/// </summary>
/// <remarks>
/// Each instance knows only its own connections. That is enough because every
/// instance receives every event through its own broker queue, so no SignalR
/// backplane is needed.
/// </remarks>
public sealed class RealtimeConnectionRegistry
{
    private readonly ConcurrentDictionary<string, RealtimeConnection> connections = new(StringComparer.Ordinal);

    public int Count => connections.Count;

    public void Add(RealtimeConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        connections[connection.ConnectionId] = connection;
    }

    public void Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    public IReadOnlyList<RealtimeConnection> GetAll() => [.. connections.Values];

    public IReadOnlyList<string> GetConnectionIdsThatCanSee(IReadOnlyCollection<Guid> partitionGuids)
    {
        ArgumentNullException.ThrowIfNull(partitionGuids);

        if (partitionGuids.Count == 0)
        {
            return [];
        }

        return [.. connections.Values
            .Where(connection => connection.CanSee(partitionGuids))
            .Select(connection => connection.ConnectionId)];
    }
}
