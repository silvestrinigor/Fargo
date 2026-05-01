namespace Fargo.Api.Events;

/// <summary>
/// Manages a real-time SignalR event hub connection. Subscribe to events before calling
/// <see cref="ConnectAsync"/>; subscriptions are applied at connection time.
/// </summary>
public interface IFargoEventHub : IAsyncDisposable
{
    /// <summary>Establishes the hub connection to the Fargo event endpoint.</summary>
    Task ConnectAsync(string serverUrl, Func<Task<string?>> tokenProvider, CancellationToken cancellationToken = default);

    /// <summary>Stops the hub connection.</summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>Registers a server-to-client handler for <paramref name="methodName"/>.</summary>
    void On<T1>(string methodName, Action<T1> handler);

    /// <inheritdoc cref="On{T1}(string, Action{T1})"/>
    void On<T1, T2>(string methodName, Action<T1, T2> handler);

    /// <inheritdoc cref="On{T1}(string, Action{T1})"/>
    void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler);

    /// <summary>Invokes a server hub method with a single <see cref="Guid"/> argument.</summary>
    Task InvokeAsync(string methodName, Guid arg, CancellationToken cancellationToken = default);
}
