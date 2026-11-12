using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Idempotency;
using Fargo.ServiceMaintenance.Options;
using Fargo.ServiceMaintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Fargo.ServiceMaintenance.Tests;

public class IdempotencyKeyPurgeServiceTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly ICommandHandler<PurgeExpiredIdempotencyKeysCommand, int> handler =
        Substitute.For<ICommandHandler<PurgeExpiredIdempotencyKeysCommand, int>>();

    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private readonly SemaphoreSlim calls = new(0);

    private readonly IdempotencyKeyPurgeOptions settings = new()
    {
        PurgeInterval = TimeSpan.FromMinutes(10),
        BatchSize = 250,
    };

    public IdempotencyKeyPurgeServiceTests()
    {
        handler
            .HandleAsync(Arg.Any<PurgeExpiredIdempotencyKeysCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0))
            .AndDoes(_ => calls.Release());
    }

    private IdempotencyKeyPurgeService CreateService()
    {
        var services = new ServiceCollection();

        services.AddSingleton(handler);

        return new IdempotencyKeyPurgeService(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            Microsoft.Extensions.Options.Options.Create(settings),
            timeProvider,
            NullLogger<IdempotencyKeyPurgeService>.Instance);
    }

    private Task WaitForCallAsync() => calls.WaitAsync(Timeout);

    [Fact]
    public async Task PurgeOnceAsync_ShouldPurgeWithTheCurrentTimeAndTheBatchSize()
    {
        await CreateService().PurgeOnceAsync(settings, CancellationToken.None);

        await handler.Received(1).HandleAsync(
            Arg.Is<PurgeExpiredIdempotencyKeysCommand>(c =>
                c.Now == timeProvider.GetUtcNow() && c.BatchSize == 250),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeOnceAsync_WhenThePurgeFails_ShouldNotThrow()
    {
        handler
            .HandleAsync(Arg.Any<PurgeExpiredIdempotencyKeysCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new InvalidOperationException("database is down"));

        await CreateService().PurgeOnceAsync(settings, CancellationToken.None);
    }

    [Fact]
    public async Task PurgeOnceAsync_WhenCancelled_ShouldPropagateTheCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        handler
            .HandleAsync(Arg.Any<PurgeExpiredIdempotencyKeysCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(call =>
            {
                call.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Task.FromResult(0);
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateService().PurgeOnceAsync(settings, cancellation.Token));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPurgeAtStartupAndThenOnEveryInterval()
    {
        var service = CreateService();

        await service.StartAsync(CancellationToken.None);

        try
        {
            await WaitForCallAsync();
            Assert.Equal(1, handler.ReceivedCalls().Count());

            timeProvider.Advance(settings.PurgeInterval);
            await WaitForCallAsync();
            Assert.Equal(2, handler.ReceivedCalls().Count());

            timeProvider.Advance(settings.PurgeInterval);
            await WaitForCallAsync();
            Assert.Equal(3, handler.ReceivedCalls().Count());
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotPurgeBeforeTheIntervalHasPassed()
    {
        var service = CreateService();

        await service.StartAsync(CancellationToken.None);

        try
        {
            await WaitForCallAsync();

            timeProvider.Advance(settings.PurgeInterval - TimeSpan.FromSeconds(1));

            Assert.False(await calls.WaitAsync(TimeSpan.FromMilliseconds(200)));
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenAPurgeFails_ShouldKeepGoing()
    {
        var failures = 0;

        handler
            .HandleAsync(Arg.Any<PurgeExpiredIdempotencyKeysCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls.Release();

                return Interlocked.Increment(ref failures) == 1
                    ? Task.FromException<int>(new InvalidOperationException("database is down"))
                    : Task.FromResult(0);
            });

        var service = CreateService();

        await service.StartAsync(CancellationToken.None);

        try
        {
            await WaitForCallAsync();

            timeProvider.Advance(settings.PurgeInterval);
            await WaitForCallAsync();

            Assert.Equal(2, Volatile.Read(ref failures));
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }
}
