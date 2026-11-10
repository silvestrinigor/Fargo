using Fargo.Application.IntegrationEvents;
using Fargo.Application.Operations.Outbox;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace Fargo.Infrastructure.Tests.Outbox;

/// <summary>
/// Outbox publish and purge tests against real PostgreSQL: the claim query uses
/// raw SQL with <c>FOR UPDATE SKIP LOCKED</c>, which the in-memory provider
/// cannot run.
/// </summary>
public class OutboxRelayTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var context = postgres.CreateContext();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE outbox_messages RESTART IDENTITY");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static PublishOutboxBatchCommandHandler CreatePublisher(FargoDbContext context, IEventPublisher publisher)
        => new(
            new OutboxRepository(context),
            publisher,
            TimeProvider.System,
            NullLogger<PublishOutboxBatchCommandHandler>.Instance);

    private static PurgeProcessedOutboxCommandHandler CreatePurger(FargoDbContext context)
        => new(
            new OutboxRepository(context),
            NullLogger<PurgeProcessedOutboxCommandHandler>.Instance);

    private async Task<List<Guid>> AddMessagesAsync(int count)
    {
        await using var context = postgres.CreateContext();

        var articleGuids = new List<Guid>();

        for (var i = 0; i < count; i++)
        {
            var articleGuid = Guid.NewGuid();
            articleGuids.Add(articleGuid);

            context.OutboxMessages.Add(OutboxMessage.FromArticleEvent(new ArticleEventMessage(
                Guid.CreateVersion7(), ArticleEventTypes.Edited, DateTimeOffset.UtcNow, articleGuid)));

            // One save per message so the sequence follows insertion order.
            await context.SaveChangesAsync();
        }

        return articleGuids;
    }

    private async Task<List<OutboxMessage>> ReadOutboxAsync()
    {
        await using var context = postgres.CreateContext();

        return await context.OutboxMessages.AsNoTracking().OrderBy(m => m.Sequence).ToListAsync();
    }

    [Fact]
    public async Task PublishBatch_ShouldPublishInSequenceOrderAndMarkProcessed()
    {
        var articleGuids = await AddMessagesAsync(3);
        var publisher = new RecordingPublisher();

        await using var context = postgres.CreateContext();
        var result = await CreatePublisher(context, publisher).HandleAsync(new PublishOutboxBatchCommand(100));

        Assert.Equal(new OutboxBatchResult(3, 3, false), result);

        var messages = await ReadOutboxAsync();
        Assert.Equal(messages.Select(m => m.EventId), publisher.Published.Select(e => e.EventId));
        Assert.Equal(articleGuids, messages.Select(m => m.AggregateGuid));
        Assert.All(messages, m => Assert.NotNull(m.ProcessedAt));
    }

    [Fact]
    public async Task PublishBatch_ShouldPublishTheStoredBodyUnchanged()
    {
        await AddMessagesAsync(1);
        var stored = Assert.Single(await ReadOutboxAsync());
        var publisher = new RecordingPublisher();

        await using var context = postgres.CreateContext();
        await CreatePublisher(context, publisher).HandleAsync(new PublishOutboxBatchCommand(100));

        var published = Assert.Single(publisher.Published);
        Assert.Equal(stored.Body, published.Body);
        Assert.Equal(ArticleEventTypes.Edited, published.EventType);
        Assert.Equal(ArticleEventMessage.SchemaVersion, published.SchemaVersion);
    }

    [Fact]
    public async Task PublishBatch_WhenOutboxEmpty_ShouldDoNothing()
    {
        var publisher = new RecordingPublisher();

        await using var context = postgres.CreateContext();
        var result = await CreatePublisher(context, publisher).HandleAsync(new PublishOutboxBatchCommand(100));

        Assert.Equal(new OutboxBatchResult(0, 0, false), result);
        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task PublishBatch_WhenPublishFails_ShouldStopRecordTheErrorAndRetryInOrderLater()
    {
        await AddMessagesAsync(3);
        var publisher = new RecordingPublisher { FailOnCall = 2 };

        await using (var context = postgres.CreateContext())
        {
            var result = await CreatePublisher(context, publisher).HandleAsync(new PublishOutboxBatchCommand(100));

            Assert.Equal(new OutboxBatchResult(3, 1, true), result);
        }

        var afterFailure = await ReadOutboxAsync();
        Assert.NotNull(afterFailure[0].ProcessedAt);
        Assert.Null(afterFailure[1].ProcessedAt);
        Assert.Equal(1, afterFailure[1].Attempts);
        Assert.Contains("Simulated broker failure", afterFailure[1].LastError);
        Assert.NotNull(afterFailure[1].LastAttemptAt);
        Assert.Null(afterFailure[2].ProcessedAt);
        Assert.Equal(0, afterFailure[2].Attempts);

        var retryPublisher = new RecordingPublisher();

        await using (var context = postgres.CreateContext())
        {
            var result = await CreatePublisher(context, retryPublisher).HandleAsync(new PublishOutboxBatchCommand(100));

            Assert.Equal(new OutboxBatchResult(2, 2, false), result);
        }

        Assert.Equal(
            afterFailure.Skip(1).Select(m => m.EventId),
            retryPublisher.Published.Select(e => e.EventId));
        Assert.All(await ReadOutboxAsync(), m => Assert.NotNull(m.ProcessedAt));
    }

    [Fact]
    public async Task PublishBatch_WhenTwoRelaysRunConcurrently_ShouldNeverPublishTheSameMessageTwice()
    {
        await AddMessagesAsync(3);

        // Relay A claims the first two rows, then holds their locks while it
        // waits inside the first publish.
        var blockingPublisher = new BlockingPublisher();

        await using var contextA = postgres.CreateContext();
        var relayA = CreatePublisher(contextA, blockingPublisher).HandleAsync(new PublishOutboxBatchCommand(2));

        await blockingPublisher.Entered.Task.WaitAsync(TimeSpan.FromSeconds(30));

        // Relay B skips the locked rows and takes only the third.
        var publisherB = new RecordingPublisher();

        await using (var contextB = postgres.CreateContext())
        {
            var resultB = await CreatePublisher(contextB, publisherB).HandleAsync(new PublishOutboxBatchCommand(100));

            Assert.Equal(new OutboxBatchResult(1, 1, false), resultB);
        }

        blockingPublisher.Release.SetResult();

        var resultA = await relayA.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.Equal(new OutboxBatchResult(2, 2, false), resultA);

        var all = blockingPublisher.Published.Concat(publisherB.Published).Select(e => e.EventId).ToList();
        Assert.Equal(3, all.Count);
        Assert.Equal(3, all.Distinct().Count());
        Assert.All(await ReadOutboxAsync(), m => Assert.NotNull(m.ProcessedAt));
    }

    [Fact]
    public async Task Purge_ShouldDeleteOnlyProcessedMessagesBeforeTheCutoff()
    {
        await AddMessagesAsync(3);

        await using (var context = postgres.CreateContext())
        {
            await CreatePublisher(context, new RecordingPublisher()).HandleAsync(new PublishOutboxBatchCommand(2));
        }

        await using (var context = postgres.CreateContext())
        {
            var deleted = await CreatePurger(context)
                .HandleAsync(new PurgeProcessedOutboxCommand(DateTimeOffset.UtcNow.AddMinutes(1)));

            Assert.Equal(2, deleted);
        }

        var remaining = Assert.Single(await ReadOutboxAsync());
        Assert.Null(remaining.ProcessedAt);
    }

    private sealed class RecordingPublisher : IEventPublisher
    {
        private int calls;

        public int? FailOnCall { get; init; }

        public ConcurrentQueue<OutboundEvent> Published { get; } = new();

        public Task PublishAsync(OutboundEvent outboundEvent, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref calls) == FailOnCall)
            {
                throw new InvalidOperationException("Simulated broker failure.");
            }

            Published.Enqueue(outboundEvent);

            return Task.CompletedTask;
        }
    }

    private sealed class BlockingPublisher : IEventPublisher
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ConcurrentQueue<OutboundEvent> Published { get; } = new();

        public async Task PublishAsync(OutboundEvent outboundEvent, CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult();

            await Release.Task.WaitAsync(cancellationToken);

            Published.Enqueue(outboundEvent);
        }
    }
}
