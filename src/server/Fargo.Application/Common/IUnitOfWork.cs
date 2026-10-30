namespace Fargo.Application.Common;

/// <summary>
/// Represents a unit of work responsible for committing changes
/// made during the execution of an application operation.
/// </summary>
/// <remarks>
/// The unit of work coordinates the persistence of changes across
/// repositories and ensures that they are committed as a single
/// transaction.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all changes made within the current unit of work.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="work"/> inside an explicit database transaction,
    /// persists the pending changes, and commits atomically. The transaction is
    /// rolled back if <paramref name="work"/> or the save throws.
    /// </summary>
    /// <param name="work">
    /// The operation to execute within the transaction. Any repository locks it
    /// acquires (for example a transaction-scoped advisory lock) are held for the
    /// full duration of the transaction, which is required to make a
    /// read-check-write sequence atomic under concurrency.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default);
}
