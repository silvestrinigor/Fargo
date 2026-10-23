using Fargo.Core.Identity;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class AuthenticationAttemptRepositoryTests
{
    private const string Ip = "10.0.0.1";
    private const string Actor = "alice";

    [Fact]
    public async Task Add_ThenCount_ShouldReflectPersistedFailures()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new AuthenticationAttemptRepository(context);

        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: false));
        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: false));
        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: true)); // success is not counted
        await context.SaveChangesAsync();

        var counts = await repo.GetAuthenticationAttemptFailCountAsync(Ip, Actor);

        Assert.Equal(2, counts.IpAddressAndUserIdentifierAttemptCount);
        Assert.Equal(2, counts.ActorIdentifierAttemptCount);
        Assert.Equal(2, counts.IpAddressAttemptCount);
    }

    [Fact]
    public async Task GetFailCount_ShouldSeparateByIpAndActor()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new AuthenticationAttemptRepository(context);

        // Same actor, different IPs.
        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: false));
        repo.Add(new AuthenticationAttempt(Actor, "10.0.0.2", successful: false));
        // Different actor, same IP.
        repo.Add(new AuthenticationAttempt("bob", Ip, successful: false));
        await context.SaveChangesAsync();

        var counts = await repo.GetAuthenticationAttemptFailCountAsync(Ip, Actor);

        Assert.Equal(1, counts.IpAddressAndUserIdentifierAttemptCount); // alice @ Ip
        Assert.Equal(2, counts.ActorIdentifierAttemptCount);            // alice everywhere
        Assert.Equal(2, counts.IpAddressAttemptCount);                  // anyone @ Ip
    }

    [Fact]
    public async Task GetFailCount_WhenPeriodStartInFuture_ShouldExcludeOlderAttempts()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new AuthenticationAttemptRepository(context);

        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: false));
        await context.SaveChangesAsync();

        var counts = await repo.GetAuthenticationAttemptFailCountAsync(
            Ip, Actor, periodStart: DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(0, counts.IpAddressAndUserIdentifierAttemptCount);
    }

    [Fact]
    public async Task GetLastFail_ShouldReturnMostRecentFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new AuthenticationAttemptRepository(context);

        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: false));
        await context.SaveChangesAsync();

        var last = await repo.GetLastFailAuthenticationAttemptAsync(Ip, Actor);

        Assert.NotNull(last);
        Assert.False(last!.Successful);
    }

    [Fact]
    public async Task GetLastSuccess_WhenOnlyFailures_ShouldReturnNull()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new AuthenticationAttemptRepository(context);

        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: false));
        await context.SaveChangesAsync();

        var lastSuccess = await repo.GetLastSuccessAuthenticationAttemptAsync(Ip, Actor);

        Assert.Null(lastSuccess);
    }

    [Fact]
    public async Task GetLastSuccess_ShouldReturnSuccessfulAttempt()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new AuthenticationAttemptRepository(context);

        repo.Add(new AuthenticationAttempt(Actor, Ip, successful: true));
        await context.SaveChangesAsync();

        var lastSuccess = await repo.GetLastSuccessAuthenticationAttemptAsync(Ip, Actor);

        Assert.NotNull(lastSuccess);
        Assert.True(lastSuccess!.Successful);
    }
}
