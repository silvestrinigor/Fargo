using Fargo.Application.Abstractions;
using Fargo.Application.IntegrationEvents;
using Fargo.Application.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Operations.Outbox;

/// <summary>
/// The outcome of publishing one outbox batch.
/// </summary>
/// <param name="Claimed">The number of messages claimed from the outbox.</param>
/// <param name="Published">The number of messages the broker accepted.</param>
/// <param name="Failed">Whether publishing stopped on a failure.</param>
public sealed record OutboxBatchResult(int Claimed, int Published, bool Failed);

/// <summary>
/// Claims the oldest unpublished outbox messages and publishes them, in order,
/// through the registered <see cref="IEventPublisher"/>.
/// </summary>
/// <param name="BatchSize">The maximum number of messages to claim and publish.</param>
public sealed record PublishOutboxBatchCommand(int BatchSize) : ICommand<OutboxBatchResult>;

/// <summary>
/// Publishes one batch of waiting outbox messages and marks them processed.
/// </summary>
/// <remarks>
/// <para>
/// The claimed batch runs in a single transaction owned by
/// <see cref="IOutboxBatch"/>: the messages are published one at a time and the
/// processed marks are committed together. Publishing stops at the first
/// failure, which is recorded on the row; that row and the ones after it are
/// retried by a later batch, so order is preserved.
/// </para>
/// <para>
/// Delivery is at-least-once: if the process stops after the broker accepted a
/// message but before the commit, the message is published again.
/// </para>
/// <para>
/// Because the handler owns its transaction and holds the claim locks across
/// the publish loop, it is registered directly, without the unit-of-work,
/// transaction or audit decorators.
/// </para>
/// </remarks>
public sealed class PublishOutboxBatchCommandHandler(
    IOutboxRepository repository,
    IEventPublisher publisher,
    TimeProvider timeProvider,
    ILogger<PublishOutboxBatchCommandHandler> logger)
    : ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResult>
{
    /// <inheritdoc />
    public async Task<OutboxBatchResult> HandleAsync(
        PublishOutboxBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await using var batch = await repository.ClaimBatchAsync(command.BatchSize, cancellationToken);

        var messages = batch.Messages;

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
        await batch.CommitAsync(CancellationToken.None);

        if (published > 0)
        {
            logger.OutboxBatchPublished(published, messages.Count);
        }

        return new OutboxBatchResult(messages.Count, published, failed);
    }
}
