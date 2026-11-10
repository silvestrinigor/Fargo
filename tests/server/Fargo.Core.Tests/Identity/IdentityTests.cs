using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Tokens;
using NSubstitute;

namespace Fargo.Core.Tests.Identity;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static TokenHash Hash() => new("ABCDEF0123456789");

    [Fact]
    public void Create_ShouldBeUsableAndNotRevoked()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash(), Now);

        Assert.True(token.IsUsableAt(Now));
        Assert.False(token.IsExpiredAt(Now));
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void Create_ShouldSetExpiryToTheDefaultSpanAfterCreation()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash(), Now);

        Assert.Equal(Now + RefreshToken.DefaultExpirationTimeSpan, token.ExpiresAt);
    }

    [Fact]
    public void IsExpiredAt_AtOrAfterExpiry_ShouldBeExpiredAndNotUsable()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash(), Now);

        Assert.True(token.IsExpiredAt(token.ExpiresAt));
        Assert.False(token.IsUsableAt(token.ExpiresAt));
    }

    [Fact]
    public void Revoke_ShouldMakeTokenUnusable()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash(), Now);

        token.Revoke(Now);

        Assert.False(token.IsUsableAt(Now));
        Assert.Equal(Now, token.RevokedAt);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldKeepOriginalRevokedAt()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Hash(), Now);

        token.Revoke(Now);
        token.Revoke(Now.AddMinutes(5));

        Assert.Equal(Now, token.RevokedAt);
    }
}

public class AuthenticationAttemptServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IAuthenticationAttemptRepository repository =
        Substitute.For<IAuthenticationAttemptRepository>();

    private readonly FixedTimeProvider timeProvider = new(Now);

    private AuthenticationAttemptService Service() => new(repository, timeProvider);

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
            : new AuthenticationAttempt("user", "127.0.0.1", successful: false, occurredAt.Value);

        repository
            .GetLastFailAuthenticationAttemptAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(attempt));

        repository
            .GetLastSuccessAuthenticationAttemptAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AuthenticationAttempt?>(null));
    }

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
        SetupLastFail(Now);

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.False(result.Allow);
        Assert.True(result.RetryAfter > TimeSpan.Zero);
    }

    [Fact]
    public async Task IsAllowedAsync_WhenActorThresholdAndRecentFailure_ShouldBlock()
    {
        SetupCounts(ip: 0, actor: 5, combined: 0);
        SetupLastFail(Now);

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.False(result.Allow);
    }

    [Fact]
    public async Task IsAllowedAsync_WhenThresholdButFailureOutsideWindow_ShouldAllow()
    {
        // 5+ combined failures, but the last failure is older than the 5-minute
        // window, so the time-window condition is not met and the attempt is allowed.
        SetupCounts(ip: 0, actor: 0, combined: 5);
        SetupLastFail(Now - TimeSpan.FromMinutes(6));

        var result = await Service().IsAllowedAsync("user", "127.0.0.1", CancellationToken.None);

        Assert.True(result.Allow);
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
    public async Task RegisterFailureAsync_ShouldAddFailedAttemptStampedWithNow()
    {
        await Service().RegisterFailureAsync("user", "127.0.0.1", CancellationToken.None);

        repository.Received(1).Add(Arg.Is<AuthenticationAttempt>(a => a != null && !a.Successful && a.OccuredAt == Now));
    }

    [Fact]
    public async Task RegisterSuccessAsync_ShouldAddSuccessfulAttemptStampedWithNow()
    {
        await Service().RegisterSuccessAsync("user", "127.0.0.1", CancellationToken.None);

        repository.Received(1).Add(Arg.Is<AuthenticationAttempt>(a => a != null && a.Successful && a.OccuredAt == Now));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
