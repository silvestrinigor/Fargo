namespace Fargo.Application.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="work"/> in an explicit transaction, saves the pending changes and commits
    /// atomically; rolls back if the work or the save throws.
    /// </summary>
    /// <remarks>
    /// Repository locks taken by the work (for example a transaction-scoped advisory lock) are held for
    /// the whole transaction, which makes a read-check-write sequence atomic under concurrency.
    /// </remarks>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default);
}
