using Fargo.Application.IntegrationEvents;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Outbox;

/// <summary>
/// The result of one relay batch.
/// </summary>
/// <param name="Claimed">The number of messages claimed from the outbox.</param>
/// <param name="Published">The number of messages the broker accepted.</param>
/// <param name="Failed">Whether publishing stopped on a failure.</param>
public sealed record OutboxBatchResult(int Claimed, int Published, bool Failed);

/// <summary>
/// Publishes waiting outbox messages, in order, and marks them processed.
/// </summary>
/// <remarks>
/// <para>
/// Each batch runs in one transaction: it claims the oldest unprocessed rows
/// with <c>FOR UPDATE SKIP LOCKED</c> (so concurrent relays never claim the
/// same row), publishes them one at a time, and commits the processed marks.
/// Publishing stops at the first failure, which is recorded on the row; the row
/// and the ones after it are retried by a later batch, so order is kept.
/// </para>
/// <para>
/// Delivery is at-least-once: if the process stops after the broker accepted a
/// message but before the commit, the message is published again.
/// </para>
/// </remarks>
public sealed class OutboxRelay(
    FargoDbContext context,
    IEventPublisher publisher,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxRelay> logger)
{
    /// <summary>
    /// Claims and publishes one batch of messages.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>What the batch did.</returns>
    public async Task<OutboxBatchResult> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var batchSize = options.Value.BatchSize;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await context.OutboxMessages
            .FromSql($"""
                SELECT * FROM outbox_messages
                WHERE processed_at IS NULL
                ORDER BY sequence
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return new OutboxBatchResult(0, 0, false);
        }

        var published = 0;
        var failed = false;

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(message.ToOutboundEvent(), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Shutting down: keep what was already published.
                break;
            }
            catch (Exception exception)
            {
                message.RecordFailure(timeProvider.GetUtcNow(), $"{exception.GetType().Name}: {exception.Message}");

                logger.OutboxPublishFailed(exception, message.EventId, message.EventType, message.Attempts);

                failed = true;

                break;
            }

            message.MarkProcessed(timeProvider.GetUtcNow());

            published++;
        }

        // Commit even when shutting down: the broker already holds these
        // messages, and not recording that would only publish them again.
        await context.SaveChangesAsync(CancellationToken.None);

        await transaction.CommitAsync(CancellationToken.None);

        if (published > 0)
        {
            logger.OutboxBatchPublished(published, messages.Count);
        }

        return new OutboxBatchResult(messages.Count, published, failed);
    }

    /// <summary>
    /// Deletes published messages processed before <paramref name="cutoff"/>.
    /// </summary>
    /// <param name="cutoff">Messages processed before this instant are deleted.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The number of deleted messages.</returns>
    public async Task<int> PurgeProcessedAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        var deleted = await context.OutboxMessages
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            logger.OutboxPurged(deleted, cutoff);
        }

        return deleted;
    }
}
