using Fargo.Application.Idempotency;

namespace Fargo.Application.Persistence.Repositories;

public interface IIdempotencyRecordRepository
{
    Task<IdempotencyRecord?> FindAsync(Guid actorGuid, Guid key, CancellationToken cancellationToken = default);

    void Add(IdempotencyRecord record);

    /// <summary>
    /// Deletes up to <paramref name="batchSize"/> records that expired before <paramref name="now"/> and returns
    /// how many it deleted.
    /// </summary>
    Task<int> PurgeExpiredAsync(DateTimeOffset now, int batchSize, CancellationToken cancellationToken = default);
}
