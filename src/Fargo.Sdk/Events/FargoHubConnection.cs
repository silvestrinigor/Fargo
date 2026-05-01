using Microsoft.AspNetCore.SignalR.Client;

namespace Fargo.Api.Events;

internal sealed class FargoHubConnection : IAsyncDisposable
{
    private readonly List<Action<HubConnection>> registrations = [];

    private HubConnection? connection;

    internal async Task ConnectAsync(string serverUrl, Func<Task<string?>> tokenProvider, CancellationToken ct)
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

        await connection.StartAsync(ct);
    }

    internal async Task DisconnectAsync(CancellationToken ct)
    {
        if (connection is not null)
        {
            await connection.StopAsync(ct);
        }
    }

    internal void On<T1>(string methodName, Action<T1> handler)
        => registrations.Add(c => c.On(methodName, handler));

    internal void On<T1, T2>(string methodName, Action<T1, T2> handler)
        => registrations.Add(c => c.On(methodName, handler));

    internal void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
        => registrations.Add(c => c.On(methodName, handler));

    internal Task InvokeAsync(string methodName, Guid arg, CancellationToken ct = default)
    {
        if (connection is null)
        {
            return Task.CompletedTask;
        }

        return connection.InvokeAsync(methodName, arg, ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }
    }
}
