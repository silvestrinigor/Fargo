using Fargo.Application.Persistence;

namespace Fargo.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of <see cref="IUnitOfWork"/> that coordinates persistence
    /// operations using the <see cref="FargoWriteDbContext"/>.
    /// </summary>
    /// <remarks>
    /// This class acts as a boundary for committing changes to the database.
    /// All write operations performed through repositories that share the same
    /// <see cref="FargoWriteDbContext"/> instance are persisted when
    /// <see cref="SaveChanges(CancellationToken)"/> is executed.
    /// <para>
    /// The unit of work ensures that multiple repository operations are committed
    /// as a single atomic database transaction handled internally by Entity Framework Core.
    /// </para>
    /// </remarks>
    public sealed class FargoUnitOfWork(FargoWriteDbContext fargoContext) : IUnitOfWork
    {
        /// <summary>
        /// The write database context used to persist changes.
        /// </summary>
        private readonly FargoWriteDbContext fargoContext = fargoContext;

        /// <summary>
        /// Persists all pending changes tracked by the <see cref="FargoWriteDbContext"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token used to cancel the save operation.
        /// </param>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// This method delegates the persistence operation to
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.
        /// </remarks>
        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
            => await fargoContext.SaveChangesAsync(cancellationToken);
    }
}