using Fargo.Sdk.Authentication;
using Fargo.Sdk.Events;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Events;

public sealed class FargoHubLifetimeServiceTests
{
    [Fact]
    public async Task RestoreAsync_Should_ConnectHub_When_HubLifetimeServiceIsRegistered()
    {
        var authClient = Substitute.For<IAuthenticationHttpClient>();
        var session = new AuthSession();
        var store = Substitute.For<ISessionStore>();
        var auth = new AuthenticationService(authClient, session, NullLogger<AuthenticationService>.Instance, store);
        var hub = Substitute.For<IFargoEventHub>();
        var options = new FargoSdkOptions { Server = "https://example.test" };
        using var sut = new FargoHubLifetimeService(auth, hub, options);

        store.LoadAsync(Arg.Any<CancellationToken>()).Returns(new StoredSession(
            "user1",
            "access",
            "refresh",
            DateTimeOffset.UtcNow.AddHours(1),
            false,
            [],
            []));

        await auth.RestoreAsync();

        await hub.Received(1).ConnectAsync(
            options.Server,
            Arg.Any<Func<Task<string?>>>(),
            Arg.Any<CancellationToken>());
    }
}
