using Fargo.RealtimeContracts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fargo.ClientRealtime;

/// <summary>
/// A connection to the Fargo real-time hub that raises
/// <see cref="EntityChanged"/> for each change the caller's actor can read.
/// </summary>
/// <remarks>
/// <para>
/// Nothing connects until <see cref="StartAsync"/>. Notifications carry no entity state: handle
/// <see cref="EntityChanged"/> by reading the entity again through the API.
/// </para>
/// <para>
/// <b>Tokens.</b> Every connect and reconnect asks the <see cref="IFargoRealtimeAccessTokenProvider"/>
/// for a token, and the service closes a connection when its token expires. The client never
/// refreshes tokens itself: a provider that must outlive a token should refresh it.
/// </para>
/// <para>
/// <b>Connection loss.</b> After <see cref="StartAsync"/> succeeds, the client keeps reconnecting
/// until <see cref="StopAsync"/>, whether the connection dropped or the server closed it.
/// <see cref="Reconnecting"/> is raised when it is lost and <see cref="Resynchronized"/> when it is
/// back. Events are not kept for disconnected clients, so re-read the data you show on
/// <see cref="Resynchronized"/>.
/// </para>
/// <para>
/// <b>Duplicates.</b> Delivery is at-least-once; an event identifier seen in the last
/// <see cref="FargoRealtimeClientOptions.DuplicateWindow"/> events is not raised again.
/// </para>
/// <para>
/// <b>Handlers.</b> An exception thrown by a handler is logged and does not affect the connection
/// or the other handlers.
/// </para>
/// </remarks>
public sealed class FargoRealtimeClient : IAsyncDisposable
{
    private readonly HubConnection connection;
    private readonly ReconnectPolicy reconnectPolicy;
    private readonly RecentEventIds recentEventIds;
    private readonly ILogger logger;
    private readonly Lock gate = new();

    private bool stopRequested = true;
    private CancellationTokenSource? restartCancellation;
    private Task restartTask = Task.CompletedTask;

    public FargoRealtimeClient(
        FargoRealtimeClientOptions options,
        IFargoRealtimeAccessTokenProvider tokenProvider,
        ILogger<FargoRealtimeClient>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tokenProvider);

        if (options.RealtimeBaseAddress is null)
        {
            throw new ArgumentException("RealtimeBaseAddress is required.", nameof(options));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(options.DuplicateWindow);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.MaxReconnectDelay, TimeSpan.Zero);

        this.logger = logger ?? (ILogger)NullLogger.Instance;
        reconnectPolicy = new ReconnectPolicy(options.MaxReconnectDelay);
        recentEventIds = new RecentEventIds(options.DuplicateWindow);

        HubUri = GetHubUri(options.RealtimeBaseAddress);

        connection = new HubConnectionBuilder()
            .WithUrl(HubUri, httpOptions =>
            {
                options.ConfigureHttpConnection?.Invoke(httpOptions);

                // Set last so the configured provider always supplies the token.
                httpOptions.AccessTokenProvider = () => tokenProvider.GetAccessTokenAsync();
            })
            .WithAutomaticReconnect(reconnectPolicy)
            .Build();

        connection.On<EntityChangedNotification>(FargoRealtimeHub.EntityChangedMethod, OnEntityChangedAsync);
        connection.Reconnecting += exception => RaiseAsync(Reconnecting, exception, nameof(Reconnecting));
        connection.Reconnected += _ => OnReconnectedAsync();
        connection.Closed += OnClosedAsync;
    }

    /// <summary>
    /// Raised for each change, at most once per event identifier within the
    /// duplicate window.
    /// </summary>
    public event Func<EntityChangedNotification, Task>? EntityChanged;

    /// <summary>
    /// Raised when the connection is lost, before reconnecting; the argument is
    /// the error, if any.
    /// </summary>
    public event Func<Exception?, Task>? Reconnecting;

    /// <summary>
    /// Raised when the connection is back after a loss. Events sent while
    /// disconnected were missed: re-read the data you show.
    /// </summary>
    public event Func<Task>? Resynchronized;

    /// <summary>
    /// Raised once the connection has closed after <see cref="StopAsync"/>.
    /// </summary>
    public event Func<Exception?, Task>? Closed;

    public Uri HubUri { get; }

    public HubConnectionState State => connection.State;

    /// <summary>
    /// Connects to the hub. A failure of this first connect is thrown to the caller, who decides
    /// whether to retry; only connections lost after a successful start are re-established.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            stopRequested = false;
        }

        await connection.StartAsync(cancellationToken);

        logger.Connected(HubUri);
    }

    /// <summary>
    /// Disconnects and stops reconnecting.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task pendingRestart;

        lock (gate)
        {
            stopRequested = true;
            restartCancellation?.Cancel();
            pendingRestart = restartTask;
        }

        await pendingRestart;

        await connection.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        await connection.DisposeAsync();

        lock (gate)
        {
            restartCancellation?.Dispose();
            restartCancellation = null;
        }
    }

    /// <summary>
    /// Keeps any path the base address already has.
    /// </summary>
    internal static Uri GetHubUri(Uri realtimeBaseAddress)
    {
        var baseText = realtimeBaseAddress.AbsoluteUri.TrimEnd('/');

        return new Uri(baseText + FargoRealtimeHub.Path);
    }

    private async Task OnEntityChangedAsync(EntityChangedNotification notification)
    {
        if (!recentEventIds.TryAdd(notification.EventId))
        {
            logger.DuplicateDropped(notification.EventId);

            return;
        }

        await RaiseAsync(EntityChanged, notification, nameof(EntityChanged));
    }

    private async Task OnReconnectedAsync()
    {
        logger.Reconnected(HubUri);

        await RaiseAsync(Resynchronized, nameof(Resynchronized));
    }

    private async Task OnClosedAsync(Exception? exception)
    {
        bool restart;

        lock (gate)
        {
            restart = !stopRequested;

            if (restart)
            {
                // Closed without StopAsync: the server closed the connection
                // (for example when the token expired) or automatic reconnect
                // ended. Start again in the background.
                restartCancellation?.Dispose();
                restartCancellation = new CancellationTokenSource();
                restartTask = RestartAsync(restartCancellation.Token);
            }
        }

        if (restart)
        {
            logger.ConnectionLost(exception, HubUri);

            await RaiseAsync(Reconnecting, exception, nameof(Reconnecting));

            return;
        }

        await RaiseAsync(Closed, exception, nameof(Closed));
    }

    private async Task RestartAsync(CancellationToken cancellationToken)
    {
        // Leave the Closed callback before connecting again.
        await Task.Yield();

        for (long attempt = 0; !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await Task.Delay(reconnectPolicy.GetDelay(attempt), cancellationToken);

                await connection.StartAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.ReconnectFailed(exception, attempt + 1);

                continue;
            }

            logger.Reconnected(HubUri);

            // Not awaited: a handler may call StopAsync, which waits for this
            // task. RaiseAsync never throws.
            _ = RaiseAsync(Resynchronized, nameof(Resynchronized));

            return;
        }
    }

    private async Task RaiseAsync<T>(Func<T, Task>? handlers, T argument, string eventName)
    {
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<Func<T, Task>>())
        {
            try
            {
                await handler(argument);
            }
            catch (Exception exception)
            {
                logger.HandlerFailed(exception, eventName);
            }
        }
    }

    private async Task RaiseAsync(Func<Task>? handlers, string eventName)
    {
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<Func<Task>>())
        {
            try
            {
                await handler();
            }
            catch (Exception exception)
            {
                logger.HandlerFailed(exception, eventName);
            }
        }
    }
}
