using Microsoft.AspNetCore.Http.Connections.Client;

namespace Fargo.ClientRealtime;

/// <summary>
/// Configuration of a <see cref="FargoRealtimeClient"/>.
/// </summary>
public sealed class FargoRealtimeClientOptions
{
    /// <summary>
    /// Gets or sets the base address of the real-time service (not the API).
    /// The hub path is appended to it, keeping any path it already has.
    /// </summary>
    public Uri? RealtimeBaseAddress { get; set; }

    /// <summary>
    /// Gets or sets how many recent event identifiers are remembered to drop
    /// duplicate deliveries. Defaults to 1000; 0 disables the filter.
    /// </summary>
    public int DuplicateWindow { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the longest wait between reconnect attempts. The wait starts
    /// at zero, then doubles from one second up to this value, and attempts
    /// never stop until <see cref="FargoRealtimeClient.StopAsync"/>.
    /// Defaults to thirty seconds.
    /// </summary>
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets an optional hook to adjust the SignalR connection, for
    /// example the transports, extra headers, or a test message handler.
    /// The access token is always supplied by the
    /// <see cref="ClientAuthentication.IFargoAccessTokenSource"/>.
    /// </summary>
    public Action<HttpConnectionOptions>? ConfigureHttpConnection { get; set; }
}
