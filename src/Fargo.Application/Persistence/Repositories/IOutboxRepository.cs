using Fargo.Application.IntegrationEvents;

namespace Fargo.Application.Persistence.Repositories;

/// <summary>
/// A batch of outbox messages claimed by one relay. The claim is held until the
/// batch is committed or disposed.
/// </summary>
public interface IOutboxBatch : IAsyncDisposable
{
    /// <summary>
    /// Tracked, oldest first: changes made through <see cref="OutboxMessage.MarkProcessed"/> and
    /// <see cref="OutboxMessage.RecordFailure"/> are saved by <see cref="CommitAsync"/>.
    /// </summary>
    IReadOnlyList<OutboxMessage> Messages { get; }

    /// <summary>
    /// Saves the changes made to the claimed messages and releases the claim. Disposing the batch
    /// without committing discards the changes.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
}

public interface IOutboxRepository
{
    /// <summary>
    /// Claims the oldest unprocessed messages so that concurrent relays never claim the same message.
    /// </summary>
    Task<IOutboxBatch> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<int> PurgeProcessedAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
}
