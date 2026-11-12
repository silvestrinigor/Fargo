using Fargo.Application.Abstractions;
using Fargo.Application.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Operations.Outbox;

public sealed record PurgeProcessedOutboxCommand(DateTimeOffset Cutoff) : ICommand<int>, INonAuditedCommand;

public sealed class PurgeProcessedOutboxCommandHandler(
    IOutboxRepository repository,
    ILogger<PurgeProcessedOutboxCommandHandler> logger)
    : ICommandHandler<PurgeProcessedOutboxCommand, int>
{
    public async Task<int> HandleAsync(
        PurgeProcessedOutboxCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var deleted = await repository.PurgeProcessedAsync(command.Cutoff, cancellationToken);

        if (deleted > 0)
        {
            logger.OutboxPurged(deleted, command.Cutoff);
        }

        return deleted;
    }
}
