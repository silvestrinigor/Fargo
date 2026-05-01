using Microsoft.AspNetCore.SignalR.Client;

namespace Fargo.Api.Events;

/// <summary>
/// Default implementation of <see cref="IFargoEventHub"/>. Register event handlers before
/// calling <see cref="ConnectAsync"/>; they are applied to the connection at that point.
/// </summary>
public sealed class FargoEventHub : IFargoEventHub
{
    private readonly List<Action<HubConnection>> registrations = [];

    private HubConnection? connection;

    /// <inheritdoc/>
    public async Task ConnectAsync(string serverUrl, Func<Task<string?>> tokenProvider, CancellationToken cancellationToken = default)
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }

        connection = new HubConnectionBuilder()
            .WithUrl(serverUrl.TrimEnd('/') + "/hub/events", opts =>
            {
                opts.AccessTokenProvider = tokenProvider;
            })
            .WithAutomaticReconnect()
            .Build();

        foreach (var registration in registrations)
        {
            registration(connection);
        }

        await connection.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (connection is not null)
        {
            await connection.StopAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public void On<T1>(string methodName, Action<T1> handler)
        => registrations.Add(c => c.On(methodName, handler));

    /// <inheritdoc/>
    public void On<T1, T2>(string methodName, Action<T1, T2> handler)
        => registrations.Add(c => c.On(methodName, handler));

    /// <inheritdoc/>
    public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
        => registrations.Add(c => c.On(methodName, handler));

    /// <inheritdoc/>
    public Task InvokeAsync(string methodName, Guid arg, CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return Task.CompletedTask;
        }

        return connection.InvokeAsync(methodName, arg, cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }
    }
}
