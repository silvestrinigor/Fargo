using Fargo.Application.Idempotency;
using Fargo.Application.Persistence.Repositories;
using Fargo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class IdempotencyRecordRepository(FargoDbContext context) : IIdempotencyRecordRepository
{
    public Task<IdempotencyRecord?> FindAsync(Guid actorGuid, Guid key, CancellationToken cancellationToken = default)
        => context.IdempotencyRecords
            .SingleOrDefaultAsync(r => r.ActorGuid == actorGuid && r.Key == key, cancellationToken);

    public void Add(IdempotencyRecord record) => context.IdempotencyRecords.Add(record);

    /// <remarks>
    /// PostgreSQL cannot limit a <c>DELETE</c> directly, so the batch is chosen by <c>ctid</c> in a subquery. The
    /// raw query repeats the table name of <see cref="IdempotencyRecordConfiguration.TableName"/>.
    /// </remarks>
    public Task<int> PurgeExpiredAsync(DateTimeOffset now, int batchSize, CancellationToken cancellationToken = default)
        => context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            DELETE FROM idempotency_keys
            WHERE ctid IN (
                SELECT ctid FROM idempotency_keys
                WHERE expires_at < {now}
                ORDER BY expires_at
                LIMIT {batchSize})
            """,
            cancellationToken);
}
