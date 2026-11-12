namespace Fargo.ServiceRealtime.Hubs;

/// <summary>
/// The methods the server calls on connected clients.
/// </summary>
public interface IFargoRealtimeClient
{
    /// <summary>
    /// Sent when an entity the client's actor can read changes. The client
    /// handles it as <c>EntityChanged</c>.
    /// </summary>
    /// <param name="notification">The change.</param>
    Task EntityChanged(EntityChangedNotification notification);
}
