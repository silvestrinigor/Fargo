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

    /// <summary>
    /// Runs <paramref name="work"/> in a transaction and commits the pending changes, rolling back on failure.
    /// </summary>
    /// <remarks>
    /// Wrapped in the context's execution strategy, so a transient failure retries the whole unit (the strategy
    /// must own the transaction boundary when retries are enabled). A transaction-scoped database lock taken by
    /// <paramref name="work"/> is held until the transaction ends.
    /// </remarks>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        var strategy = fargoContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await fargoContext.Database.BeginTransactionAsync(ct);

            await work(ct);

            await fargoContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }, cancellationToken);
    }
}
