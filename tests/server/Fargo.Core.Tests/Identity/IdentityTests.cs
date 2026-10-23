using Fargo.Core.Identity;
using NSubstitute;

namespace Fargo.Core.Tests.Identity;

public class RefreshTokenTests
{
    private static TokenHash Hash() => new("ABCDEF0123456789");

    [Fact]
    public void Create_ShouldBeUsableAndNotRevoked()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash());

        Assert.True(token.IsUsable);
        Assert.False(token.IsExpired);
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void Create_ShouldSetDefaultExpiryInTheFuture()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash());

        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Revoke_ShouldMakeTokenUnusable()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash());

        token.Revoke();

        Assert.False(token.IsUsable);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldKeepOriginalRevokedAt()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash());

        token.Revoke();
        var firstRevokedAt = token.RevokedAt;
        token.Revoke();

        Assert.Equal(firstRevokedAt, token.RevokedAt);
    }
}

public class AuthenticationAttemptServiceTests
{
    private readonly IAuthenticationAttemptRepository repository =
        Substitute.For<IAuthenticationAttemptRepository>();

    private AuthenticationAttemptService Service() => new(repository);

    private void SetupCounts(int ip, int actor, int combined)
    {
        repository
            .GetAuthenticationAttemptFailCountAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuthenticationAttemptCountInformation(ip, actor, combined)));
    }

    private void SetupLastFail(DateTimeOffset? occurredAt)
    {
        var attempt = occurredAt is null
            ? null
            : BuildAttempt(occurredAt.Value);

        repository
            .GetLastFailAuthenticationAttemptAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(attempt));

        repository
            .GetLastSuccessAuthenticationAttemptAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AuthenticationAttempt?>(null));
    }

    // Build an attempt with a controlled OccuredAt (private init) via reflection-free path:
    // OccuredAt is set from UtcNow in the ctor, so we approximate "recent" by using the ctor
    // and asserting on behavior rather than exact timing. For "recent" cases we create now.
    private static AuthenticationAttempt BuildAttempt(DateTimeOffset _)
        => new("user", "127.0.0.1", successful: false);

    [Fact]
    public async Task IsAllowedAsync_WhenNoFailures_ShouldAllow()
    {
        SetupCounts(ip: 0, actor: 0, combined: 0);
        SetupLastFail(null);

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.True(result.Allow);
        Assert.Equal(TimeSpan.Zero, result.RetryAfter);
    }

    [Fact]
    public async Task IsAllowedAsync_WhenCombinedThresholdAndRecentFailure_ShouldBlock()
    {
        // 5+ combined IP+user failures with a recent (now) last failure -> blocked.
        SetupCounts(ip: 0, actor: 0, combined: 5);
        SetupLastFail(DateTimeOffset.UtcNow);

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.False(result.Allow);
        Assert.True(result.RetryAfter > TimeSpan.Zero);
    }

    [Fact]
    public async Task IsAllowedAsync_WhenActorThresholdAndRecentFailure_ShouldBlock()
    {
        SetupCounts(ip: 0, actor: 5, combined: 0);
        SetupLastFail(DateTimeOffset.UtcNow);

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.False(result.Allow);
    }

    [Fact]
    public async Task IsAllowedAsync_WhenCountsHighButNoRecentFailure_ShouldAllow()
    {
        // Counts exceed thresholds but there is no last failure timestamp, so the
        // time-window conditions are not met and the attempt is allowed.
        SetupCounts(ip: 50, actor: 50, combined: 50);
        SetupLastFail(null);

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.True(result.Allow);
    }

    [Fact]
    public async Task RegisterFailureAsync_ShouldAddFailedAttempt()
    {
        await Service().RegisterFailureAsync("user", "127.0.0.1", CancellationToken.None);

        repository.Received(1).Add(Arg.Is<AuthenticationAttempt>(a => a != null && !a.Successful));
    }

    [Fact]
    public async Task RegisterSuccessAsync_ShouldAddSuccessfulAttempt()
    {
        await Service().RegisterSuccessAsync("user", "127.0.0.1", CancellationToken.None);

        repository.Received(1).Add(Arg.Is<AuthenticationAttempt>(a => a != null && a.Successful));
    }
}
