using Fargo.Application.IntegrationEvents;
using Fargo.Application.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(FargoDbContext context) : IOutboxRepository
{
    /// <remarks>
    /// <c>FOR UPDATE SKIP LOCKED</c> locks the claimed rows until the surrounding transaction ends and
    /// skips rows another relay holds. The raw query repeats the table name of
    /// <see cref="Configurations.OutboxMessageConfiguration.TableName"/>, so keep the two in sync.
    /// </remarks>
    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages
            .FromSql($"""
                SELECT * FROM outbox_messages
                WHERE processed_at IS NULL
                ORDER BY sequence
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> PurgeProcessedAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        return context.OutboxMessages
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
