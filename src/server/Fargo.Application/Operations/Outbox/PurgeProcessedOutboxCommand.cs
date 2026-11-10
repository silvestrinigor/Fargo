using Fargo.Application.Abstractions;
using Fargo.Application.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Operations.Outbox;

/// <summary>
/// Deletes published outbox messages that were processed before a cutoff, so the
/// outbox does not grow without bound.
/// </summary>
/// <param name="Cutoff">Messages processed before this instant are deleted.</param>
public sealed record PurgeProcessedOutboxCommand(DateTimeOffset Cutoff) : ICommand<int>;

/// <summary>
/// Purges processed outbox messages older than the requested cutoff.
/// </summary>
public sealed class PurgeProcessedOutboxCommandHandler(
    IOutboxRepository repository,
    ILogger<PurgeProcessedOutboxCommandHandler> logger)
    : ICommandHandler<PurgeProcessedOutboxCommand, int>
{
    /// <inheritdoc />
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
