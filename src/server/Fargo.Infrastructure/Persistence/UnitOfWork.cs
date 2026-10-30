using Fargo.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Implementation of <see cref="IUnitOfWork"/> that coordinates persistence
/// operations using the <see cref="FargoDbContext"/>.
/// </summary>
public sealed class UnitOfWork(FargoDbContext fargoContext, ILogger<IUnitOfWork> logger) : IUnitOfWork
{
    private readonly FargoDbContext fargoContext = fargoContext;

    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the save operation.
    /// </param>
    /// <returns>
    /// The number of state entries written to the database.
    /// </returns>
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
    /// Runs <paramref name="work"/> inside an explicit transaction and commits
    /// the pending changes atomically, rolling back on failure.
    /// </summary>
    /// <remarks>
    /// Wrapped in the context's execution strategy so a transient failure retries
    /// the whole unit (the strategy owns the transaction boundary, per EF Core's
    /// requirement when retries are enabled). Any transaction-scoped database lock
    /// acquired by <paramref name="work"/> is held until this transaction commits
    /// or rolls back.
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
