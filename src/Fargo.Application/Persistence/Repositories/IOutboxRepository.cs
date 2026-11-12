using Fargo.Application.IntegrationEvents;

namespace Fargo.Application.Persistence.Repositories;

/// <summary>
/// A batch of outbox messages claimed by one relay. The claim is held until the
/// batch is committed or disposed.
/// </summary>
public interface IOutboxBatch : IAsyncDisposable
{
    /// <summary>
    /// Gets the claimed messages, oldest first. They are tracked, so changes
    /// made through <see cref="OutboxMessage.MarkProcessed"/> and
    /// <see cref="OutboxMessage.RecordFailure"/> are saved by
    /// <see cref="CommitAsync"/>.
    /// </summary>
    IReadOnlyList<OutboxMessage> Messages { get; }

    /// <summary>
    /// Saves the changes made to the claimed messages and releases the claim.
    /// Disposing the batch without committing discards the changes.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads and maintains the messages waiting in the transactional outbox.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Claims the oldest unprocessed messages so that concurrent relays never
    /// claim the same message.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to claim.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The claimed batch. Dispose it when done.</returns>
    Task<IOutboxBatch> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes published messages processed before <paramref name="cutoff"/>.
    /// </summary>
    /// <param name="cutoff">Messages processed before this instant are deleted.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The number of deleted messages.</returns>
    Task<int> PurgeProcessedAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
}
