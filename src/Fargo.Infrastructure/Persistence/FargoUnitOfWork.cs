using Fargo.Application;
using Fargo.Application.Authentication;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

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
/// <para>
/// Before persisting changes, this unit of work applies auditing metadata
/// to modified auditable entities tracked by the context.
/// </para>
/// </remarks>
public sealed class FargoUnitOfWork(
    FargoDbContext fargoContext,
    IAuditPrincipal auditPrincipal) : IUnitOfWork
{
    /// <summary>
    /// The write database context used to persist changes.
    /// </summary>
    private readonly FargoDbContext fargoContext = fargoContext;

    /// <summary>
    /// Provides the audit origin for persistence writes.
    /// </summary>
    private readonly IAuditPrincipal auditPrincipal = auditPrincipal;

    /// <summary>
    /// Persists all pending changes tracked by the <see cref="FargoDbContext"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the save operation.
    /// </param>
    /// <returns>
    /// The number of state entries written to the database.
    /// </returns>
    /// <remarks>
    /// This method applies auditing metadata to modified entities before
    /// delegating the persistence operation to
    /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.
    /// </remarks>
    public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        fargoContext.ChangeTracker.ApplyAuditing(auditPrincipal.ActorGuid);

        return await fargoContext.SaveChangesAsync(cancellationToken);
    }
}
