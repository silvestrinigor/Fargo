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

    /// <summary>
    /// Gets the number of open connections.
    /// </summary>
    public int Count => connections.Count;

    /// <summary>
    /// Registers an opened connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public void Add(RealtimeConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        connections[connection.ConnectionId] = connection;
    }

    /// <summary>
    /// Forgets a closed connection.
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    public void Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    /// <summary>
    /// Gets a snapshot of the open connections.
    /// </summary>
    public IReadOnlyList<RealtimeConnection> GetAll() => [.. connections.Values];

    /// <summary>
    /// Gets the connections whose actor may learn about an entity in the given
    /// partitions (see <see cref="RealtimeConnection.CanSee"/>).
    /// </summary>
    /// <param name="partitionGuids">The partitions of the entity.</param>
    /// <returns>The identifiers of the matching connections.</returns>
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
