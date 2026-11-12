namespace Fargo.Application.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="work"/> in an explicit transaction, saves the pending changes and commits
    /// atomically; rolls back if the work or the save throws.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Repository locks taken by the work (for example a transaction-scoped advisory lock or
    /// <c>FOR UPDATE</c> rows) are held for the whole transaction, which makes a read-check-write
    /// sequence atomic under concurrency.
    /// </para>
    /// <para>
    /// <paramref name="cancellationToken"/> can stop the work, but not the save and commit once the
    /// work has finished: cancelling then would only discard work that is already done, including
    /// side effects outside the database.
    /// </para>
    /// </remarks>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ExecuteInTransactionAsync(Func{CancellationToken, Task}, CancellationToken)"/>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> work,
        CancellationToken cancellationToken = default);
}
