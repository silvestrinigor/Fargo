using Fargo.Application;
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
}
