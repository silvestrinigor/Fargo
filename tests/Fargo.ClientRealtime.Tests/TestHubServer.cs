using Fargo.ClientAuthentication;
using Fargo.RealtimeContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Fargo.ClientRealtime.Tests;

/// <summary>
/// An in-process hub with the real contract, so the client is tested against a
/// real SignalR server without the Fargo service, database or broker.
/// </summary>
internal sealed class TestHubServer : IAsyncDisposable
{
    private readonly WebApplication app;

    private TestHubServer(WebApplication app)
    {
        this.app = app;
    }

    public HubState State => app.Services.GetRequiredService<HubState>();

    public IHubContext<TestHub, IFargoRealtimeClient> Hub
        => app.Services.GetRequiredService<IHubContext<TestHub, IFargoRealtimeClient>>();

    public static async Task<TestHubServer> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<HubState>();

        var app = builder.Build();
        app.MapHub<TestHub>(FargoRealtimeHub.Path);

        await app.StartAsync();

        return new TestHubServer(app);
    }

    public FargoRealtimeClient CreateClient(IFargoAccessTokenSource tokenSource, int duplicateWindow = 1000)
    {
        var server = app.GetTestServer();

        return new FargoRealtimeClient(
            new FargoRealtimeClientOptions
            {
                RealtimeBaseAddress = server.BaseAddress,
                DuplicateWindow = duplicateWindow,
                MaxReconnectDelay = TimeSpan.FromMilliseconds(200),
                ConfigureHttpConnection = options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                },
            },
            tokenSource);
    }

    public async ValueTask DisposeAsync()
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    internal sealed class HubState
    {
        public ConcurrentDictionary<string, HubCallerContext> Connections { get; } = new();

        public ConcurrentQueue<string?> AuthorizationHeaders { get; } = new();

        public int ConnectCount;
    }

    internal sealed class TestHub(HubState state) : Hub<IFargoRealtimeClient>
    {
        public override Task OnConnectedAsync()
        {
            state.AuthorizationHeaders.Enqueue(Context.GetHttpContext()?.Request.Headers.Authorization.ToString());
            state.Connections[Context.ConnectionId] = Context;
            Interlocked.Increment(ref state.ConnectCount);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            state.Connections.TryRemove(Context.ConnectionId, out _);

            return base.OnDisconnectedAsync(exception);
        }
    }
}

/// <summary>
/// Returns numbered tokens and counts how often it was asked.
/// </summary>
internal sealed class CountingTokenSource : IFargoAccessTokenSource
{
    private int calls;

    public int Calls => Volatile.Read(ref calls);

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<string?>($"token-{Interlocked.Increment(ref calls)}");
}

internal static class Wait
{
    public static async Task UntilAsync(Func<bool> condition, string because, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail($"Timed out waiting until {because}.");
            }

            await Task.Delay(20);
        }
    }
}
