using Fargo.Sdk.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Authentication;

public sealed class AuthenticationServiceTests
{
    private readonly IAuthenticationHttpClient client = Substitute.For<IAuthenticationHttpClient>();
    private readonly AuthSession session = new();
    private readonly ISessionStore store = Substitute.For<ISessionStore>();
    private readonly AuthenticationService sut;

    public AuthenticationServiceTests()
    {
        sut = new AuthenticationService(client, session, NullLogger<AuthenticationService>.Instance, store);
    }

    [Fact]
    public async Task LogOutAsync_Should_ClearLocalSession_When_RefreshTokenIsMissing()
    {
        session.SetTokens("user1", "access", null!, DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        await sut.LogOutAsync();

        Assert.False(sut.IsAuthenticated);
        await store.Received(1).ClearAsync(Arg.Any<CancellationToken>());
        await client.DidNotReceive().LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreAsync_Should_RaiseRestoredEvent_When_SessionExists()
    {
        store.LoadAsync(Arg.Any<CancellationToken>()).Returns(new StoredSession(
            "user1",
            "access",
            "refresh",
            DateTimeOffset.UtcNow.AddHours(1),
            false,
            [],
            []));

        SessionRestoredEventArgs? raisedArgs = null;
        sut.Restored += (_, args) => raisedArgs = args;

        var restored = await sut.RestoreAsync();

        Assert.True(restored);
        Assert.NotNull(raisedArgs);
        Assert.Equal("user1", raisedArgs.Nameid);
    }
}
