using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Outbox;
using Fargo.ServiceMaintenance.Options;
using Fargo.ServiceMaintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Fargo.ServiceMaintenance.Tests;

public class OutboxPurgeServiceTests
{
    private readonly ICommandHandler<PurgeProcessedOutboxCommand, int> handler =
        Substitute.For<ICommandHandler<PurgeProcessedOutboxCommand, int>>();

    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero));

    private readonly OutboxPurgeOptions settings = new() { Retention = TimeSpan.FromDays(3) };

    private OutboxPurgeService CreateService()
    {
        var services = new ServiceCollection();

        services.AddSingleton(handler);

        return new OutboxPurgeService(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            Microsoft.Extensions.Options.Options.Create(settings),
            timeProvider,
            NullLogger<OutboxPurgeService>.Instance);
    }

    [Fact]
    public async Task PurgeOnceAsync_ShouldPurgeMessagesPublishedBeforeTheRetentionCutoff()
    {
        await CreateService().PurgeOnceAsync(settings, CancellationToken.None);

        await handler.Received(1).HandleAsync(
            Arg.Is<PurgeProcessedOutboxCommand>(c => c.Cutoff == timeProvider.GetUtcNow() - TimeSpan.FromDays(3)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeOnceAsync_ShouldSwallowFailures()
    {
        handler
            .HandleAsync(Arg.Any<PurgeProcessedOutboxCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new InvalidOperationException("database down"));

        await CreateService().PurgeOnceAsync(settings, CancellationToken.None);
    }

    [Fact]
    public async Task PurgeOnceAsync_ShouldRethrowCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        handler
            .HandleAsync(Arg.Any<PurgeProcessedOutboxCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new OperationCanceledException(cancellation.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateService().PurgeOnceAsync(settings, cancellation.Token));
    }
}
