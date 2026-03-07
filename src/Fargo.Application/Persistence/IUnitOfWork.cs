namespace Fargo.Application.Persistence
{
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
        Task<int> SaveChanges(CancellationToken cancellationToken = default);
    }
}