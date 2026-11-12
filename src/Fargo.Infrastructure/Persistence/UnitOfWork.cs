using Fargo.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fargo.Infrastructure.Persistence;

public sealed class UnitOfWork(FargoDbContext fargoContext, ILogger<IUnitOfWork> logger) : IUnitOfWork
{
    private readonly FargoDbContext fargoContext = fargoContext;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = await fargoContext.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Unit of work save {changes} changes.", changes);
        }

        return changes;
    }

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        return ExecuteInTransactionAsync<bool>(
            async ct =>
            {
                await work(ct);

                return true;
            },
            cancellationToken);
    }

    /// <remarks>
    /// Wrapped in the context's execution strategy, so the strategy owns the transaction boundary. No
    /// retrying strategy is configured today; if one is enabled, a transient failure re-runs the whole
    /// <paramref name="work"/>, so it must be safe to repeat (the outbox relay would publish again).
    /// </remarks>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        var strategy = fargoContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await fargoContext.Database.BeginTransactionAsync(ct);

            var result = await work(ct);

            // The work is done: cancelling now would only throw it away (and, for the outbox, publish
            // the same messages again), so the save and commit run to completion.
            await fargoContext.SaveChangesAsync(CancellationToken.None);

            await transaction.CommitAsync(CancellationToken.None);

            return result;
        }, cancellationToken);
    }
}
