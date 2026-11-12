using Fargo.Application.IntegrationEvents;

namespace Fargo.Application.Persistence.Repositories;

public interface IOutboxRepository
{
    /// <summary>
    /// Claims the oldest unprocessed messages, oldest first and tracked, so that concurrent relays never
    /// claim the same message.
    /// </summary>
    /// <remarks>
    /// The claim is a row lock held until the surrounding transaction ends, so this must run inside one
    /// (the publish command is an <c>ITransactionalCommand</c>); otherwise the lock is released at once.
    /// Changes made through <see cref="OutboxMessage.MarkProcessed"/> and
    /// <see cref="OutboxMessage.RecordFailure"/> are saved by that transaction.
    /// </remarks>
    Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<int> PurgeProcessedAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
}
