using Fargo.Application.Abstractions;
using Fargo.Application.Idempotency;
using Fargo.Application.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Commands.Idempotency;

/// <summary>
/// Deletes the idempotency records that expired before <see cref="Now"/>, in batches so no single statement
/// holds locks for long. Returns how many it deleted.
/// </summary>
public sealed record PurgeExpiredIdempotencyKeysCommand(DateTimeOffset Now, int BatchSize = 1000)
    : ICommand<int>, INonAuditedCommand;

public sealed class PurgeExpiredIdempotencyKeysCommandHandler(
    IIdempotencyRecordRepository repository,
    ILogger<PurgeExpiredIdempotencyKeysCommandHandler> logger)
    : ICommandHandler<PurgeExpiredIdempotencyKeysCommand, int>
{
    public async Task<int> HandleAsync(
        PurgeExpiredIdempotencyKeysCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentOutOfRangeException.ThrowIfLessThan(command.BatchSize, 1);

        var total = 0;
        int deleted;

        do
        {
            deleted = await repository.PurgeExpiredAsync(command.Now, command.BatchSize, cancellationToken);

            total += deleted;
        }
        while (deleted >= command.BatchSize);

        if (total > 0)
        {
            logger.IdempotencyKeysPurged(total);
        }

        return total;
    }
}
