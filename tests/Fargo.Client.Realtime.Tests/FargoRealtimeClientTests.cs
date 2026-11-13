using Fargo.Realtime.Contracts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Fargo.Client.Realtime.Tests;

public sealed class FargoRealtimeClientTests : IAsyncLifetime
{
    private TestHubServer server = null!;

    public async Task InitializeAsync() => server = await TestHubServer.StartAsync();

    public async Task DisposeAsync() => await server.DisposeAsync();

    private static EntityChangedNotification NewNotification(Guid? eventId = null)
        => new(
            EntityChangedNotification.EntityTypes.Article,
            Guid.NewGuid(),
            EntityChangedNotification.ChangeTypes.Edited,
            eventId ?? Guid.CreateVersion7(),
            new DateTimeOffset(2026, 11, 6, 14, 3, 22, TimeSpan.Zero));

    [Fact]
    public async Task EntityChanged_ShouldDeliverTheTypedNotification()
    {
        var tokens = new CountingTokenSource();
        await using var client = server.CreateClient(tokens);
        var received = new ConcurrentQueue<EntityChangedNotification>();
        client.EntityChanged += n => { received.Enqueue(n); return Task.CompletedTask; };

        await client.StartAsync();
        var sent = NewNotification();
        await server.Hub.Clients.All.EntityChanged(sent);

        await Wait.UntilAsync(() => !received.IsEmpty, "the notification arrives");
        Assert.Equal(sent, Assert.Single(received));
        Assert.Equal(HubConnectionState.Connected, client.State);
    }

    [Fact]
    public async Task StartAsync_ShouldSendTheTokenFromTheTokenSource()
    {
        var tokens = new CountingTokenSource();
        await using var client = server.CreateClient(tokens);

        await client.StartAsync();

        await Wait.UntilAsync(() => server.State.ConnectCount == 1, "the hub sees the connection");
        Assert.Contains(server.State.AuthorizationHeaders, header => header?.StartsWith("Bearer token-", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task EntityChanged_ShouldDropADuplicateEventId()
    {
        await using var client = server.CreateClient(new CountingTokenSource());
        var received = new ConcurrentQueue<EntityChangedNotification>();
        client.EntityChanged += n => { received.Enqueue(n); return Task.CompletedTask; };
        await client.StartAsync();

        var first = NewNotification();
        var duplicate = first with { Guid = Guid.NewGuid() };
        var last = NewNotification();
        await server.Hub.Clients.All.EntityChanged(first);
        await server.Hub.Clients.All.EntityChanged(duplicate);
        await server.Hub.Clients.All.EntityChanged(last);

        await Wait.UntilAsync(() => received.Any(n => n.EventId == last.EventId), "the last notification arrives");
        Assert.Equal([first.EventId, last.EventId], received.Select(n => n.EventId));
    }

    [Fact]
    public async Task EntityChanged_WhenAHandlerThrows_ShouldKeepDelivering()
    {
        await using var client = server.CreateClient(new CountingTokenSource());
        var received = new ConcurrentQueue<EntityChangedNotification>();
        client.EntityChanged += _ => throw new InvalidOperationException("handler bug");
        client.EntityChanged += n => { received.Enqueue(n); return Task.CompletedTask; };
        await client.StartAsync();

        await server.Hub.Clients.All.EntityChanged(NewNotification());
        await server.Hub.Clients.All.EntityChanged(NewNotification());

        await Wait.UntilAsync(() => received.Count == 2, "both notifications reach the second handler");
        Assert.Equal(HubConnectionState.Connected, client.State);
    }

    [Fact]
    public async Task WhenTheServerClosesTheConnection_ShouldReconnectWithAFreshTokenAndResynchronize()
    {
        var tokens = new CountingTokenSource();
        await using var client = server.CreateClient(tokens);
        var resynchronized = 0;
        client.Resynchronized += () => { Interlocked.Increment(ref resynchronized); return Task.CompletedTask; };
        await client.StartAsync();
        await Wait.UntilAsync(() => server.State.ConnectCount == 1, "the first connection is open");
        var tokensBefore = tokens.Calls;

        // What the real-time service does when the access token expires.
        foreach (var connection in server.State.Connections.Values)
        {
            connection.Abort();
        }

        await Wait.UntilAsync(() => Volatile.Read(ref resynchronized) == 1, "the client resynchronizes");
        await Wait.UntilAsync(() => server.State.ConnectCount == 2, "the hub sees the new connection");
        Assert.True(tokens.Calls > tokensBefore, "A reconnect must ask the token source again.");

        // And it still receives events.
        var received = new ConcurrentQueue<EntityChangedNotification>();
        client.EntityChanged += n => { received.Enqueue(n); return Task.CompletedTask; };
        await server.Hub.Clients.All.EntityChanged(NewNotification());
        await Wait.UntilAsync(() => !received.IsEmpty, "a notification arrives after the reconnect");
    }

    [Fact]
    public async Task StopAsync_ShouldRaiseClosedAndNotReconnect()
    {
        await using var client = server.CreateClient(new CountingTokenSource());
        var closed = 0;
        var resynchronized = 0;
        client.Closed += _ => { Interlocked.Increment(ref closed); return Task.CompletedTask; };
        client.Resynchronized += () => { Interlocked.Increment(ref resynchronized); return Task.CompletedTask; };
        await client.StartAsync();

        await client.StopAsync();

        await Wait.UntilAsync(() => Volatile.Read(ref closed) == 1, "Closed is raised");
        await Task.Delay(500);
        Assert.Equal(0, Volatile.Read(ref resynchronized));
        Assert.Equal(1, server.State.ConnectCount);
        Assert.Equal(HubConnectionState.Disconnected, client.State);
    }

    [Fact]
    public async Task AddFargoRealtimeClient_ShouldUseTheRegisteredAccessTokenProvider()
    {
        var tokens = new CountingTokenSource();
        var services = new ServiceCollection();
        services.AddSingleton<IFargoRealtimeAccessTokenProvider>(tokens);
        services.AddFargoRealtimeClient(new Uri("http://realtime.test"));

        // The client is IAsyncDisposable only, like SignalR's HubConnection.
        await using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<FargoRealtimeClient>();
        Assert.Equal(new Uri("http://realtime.test/hubs/fargo"), client.HubUri);
    }

    [Fact]
    public async Task AddFargoRealtimeClient_WithoutAnAccessTokenProvider_ShouldFailToResolve()
    {
        var services = new ServiceCollection();
        services.AddFargoRealtimeClient(new Uri("http://realtime.test"));

        await using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<FargoRealtimeClient>());
    }

    [Fact]
    public async Task DelegateRealtimeAccessTokenProvider_ShouldReturnTheDelegatesToken()
    {
        var provider = new DelegateRealtimeAccessTokenProvider(_ => Task.FromResult<string?>("token"));

        Assert.Equal("token", await provider.GetAccessTokenAsync());
    }
}

public class RealtimeClientHelpersTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1000)]
    [InlineData(2, 2000)]
    [InlineData(3, 4000)]
    [InlineData(6, 30000)]
    [InlineData(1000, 30000)]
    public void ReconnectPolicy_ShouldStartImmediatelyThenDoubleUpToTheMaximum(long previousAttempts, int expectedMilliseconds)
    {
        var policy = new ReconnectPolicy(TimeSpan.FromSeconds(30));

        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), policy.GetDelay(previousAttempts));
    }

    [Fact]
    public void ReconnectPolicy_ShouldNeverGiveUp()
    {
        var policy = new ReconnectPolicy(TimeSpan.FromSeconds(30));

        Assert.NotNull(policy.NextRetryDelay(new RetryContext { PreviousRetryCount = 10_000 }));
    }

    [Fact]
    public void RecentEventIds_ShouldForgetTheOldestBeyondTheCapacity()
    {
        var recent = new RecentEventIds(2);
        var (a, b, c) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.True(recent.TryAdd(a));
        Assert.True(recent.TryAdd(b));
        Assert.False(recent.TryAdd(a));
        Assert.True(recent.TryAdd(c));

        // a was the oldest and has been forgotten.
        Assert.True(recent.TryAdd(a));
        Assert.False(recent.TryAdd(c));
    }

    [Fact]
    public void RecentEventIds_WithZeroCapacity_ShouldAcceptEverything()
    {
        var recent = new RecentEventIds(0);
        var id = Guid.NewGuid();

        Assert.True(recent.TryAdd(id));
        Assert.True(recent.TryAdd(id));
    }

    [Theory]
    [InlineData("http://realtime.test", "http://realtime.test/hubs/fargo")]
    [InlineData("http://realtime.test/", "http://realtime.test/hubs/fargo")]
    [InlineData("http://proxy.test/fargo", "http://proxy.test/fargo/hubs/fargo")]
    [InlineData("http://proxy.test/fargo/", "http://proxy.test/fargo/hubs/fargo")]
    public void GetHubUri_ShouldAppendTheHubPathKeepingTheBasePath(string baseAddress, string expected)
    {
        Assert.Equal(new Uri(expected), FargoRealtimeClient.GetHubUri(new Uri(baseAddress)));
    }
}
