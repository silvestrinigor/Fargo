using Fargo.Application.IntegrationEvents;
using Fargo.Application.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(FargoDbContext context) : IOutboxRepository
{
    /// <remarks>
    /// The batch runs in one transaction: the oldest unprocessed rows are
    /// claimed with <c>FOR UPDATE SKIP LOCKED</c>, so concurrent relays never
    /// claim the same row, and the locks are held until the batch is committed
    /// or disposed. The raw query repeats the table name of
    /// <see cref="Configurations.OutboxMessageConfiguration.TableName"/>, so
    /// keep the two in sync.
    /// </remarks>
    public async Task<IOutboxBatch> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
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

            return new OutboxBatch(context, transaction, messages);
        }
        catch
        {
            await transaction.DisposeAsync();

            throw;
        }
    }

    public Task<int> PurgeProcessedAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        return context.OutboxMessages
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private sealed class OutboxBatch(
        FargoDbContext context,
        IDbContextTransaction transaction,
        IReadOnlyList<OutboxMessage> messages) : IOutboxBatch
    {
        public IReadOnlyList<OutboxMessage> Messages { get; } = messages;

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
