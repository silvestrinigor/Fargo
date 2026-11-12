using Fargo.Application.Abstractions;
using Fargo.Application.IntegrationEvents;
using Fargo.Application.Operations.Outbox.Dtos;
using Fargo.Application.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Operations.Outbox;

/// <remarks>
/// Transactional: the claimed rows stay locked until the transactional decorator commits, so
/// concurrent relays never publish the same row.
/// </remarks>
public sealed record PublishOutboxBatchCommand(int BatchSize)
    : ICommand<OutboxBatchResultDto>, ITransactionalCommand, INonAuditedCommand;

/// <remarks>
/// Messages are published one at a time and stop at the first failure, which is recorded on the row;
/// that row and the ones after it are retried by a later batch, so order is preserved.
///
/// Delivery is at-least-once: if the process stops after the broker accepted a message but before
/// the commit, the message is published again. A cancellation only stops the publishing: what was
/// already published is still committed.
/// </remarks>
public sealed class PublishOutboxBatchCommandHandler(
    IOutboxRepository repository,
    IEventPublisher publisher,
    TimeProvider timeProvider,
    ILogger<PublishOutboxBatchCommandHandler> logger)
    : ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResultDto>
{
    public async Task<OutboxBatchResultDto> HandleAsync(
        PublishOutboxBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var messages = await repository.ClaimBatchAsync(command.BatchSize, cancellationToken);

        if (messages.Count == 0)
        {
            return new OutboxBatchResultDto(0, 0, false);
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
                // Shutting down: the transactional decorator still commits what was published.
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

        if (published > 0)
        {
            logger.OutboxBatchPublished(published, messages.Count);
        }

        return new OutboxBatchResultDto(messages.Count, published, failed);
    }
}
