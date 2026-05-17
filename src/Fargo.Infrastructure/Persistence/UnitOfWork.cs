using Fargo.Application;

namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Implementation of <see cref="IUnitOfWork"/> that coordinates persistence
/// operations using the <see cref="FargoDbContext"/>.
/// </summary>
/// <remarks>
/// This class acts as a boundary for committing changes to the database.
/// All write operations performed through repositories that share the same
/// <see cref="FargoDbContext"/> instance are persisted when
/// <see cref="SaveChanges(CancellationToken)"/> is executed.
/// <para>
/// The unit of work ensures that multiple repository operations are committed
/// as a single atomic database transaction handled internally by Entity Framework Core.
/// </para>
/// </remarks>
public sealed class UnitOfWork(
    FargoDbContext fargoContext
) : IUnitOfWork
{
    /// <summary>
    /// The write database context used to persist changes.
    /// </summary>
    private readonly FargoDbContext fargoContext = fargoContext;

    /// <summary>
    /// Persists all pending changes tracked by the <see cref="FargoDbContext"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the save operation.
    /// </param>
    /// <returns>
    /// The number of state entries written to the database.
    /// </returns>
    public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        => await fargoContext.SaveChangesAsync(cancellationToken);
}
