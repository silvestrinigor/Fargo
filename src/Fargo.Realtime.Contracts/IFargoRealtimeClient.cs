namespace Fargo.Realtime.Contracts;

/// <summary>
/// The methods the server calls on connected clients.
/// </summary>
public interface IFargoRealtimeClient
{
    /// <summary>
    /// Sent when an entity the client's actor can read changes. Its hub method
    /// name is <see cref="FargoRealtimeHub.EntityChangedMethod"/>.
    /// </summary>
    Task EntityChanged(EntityChangedNotification notification);
}
