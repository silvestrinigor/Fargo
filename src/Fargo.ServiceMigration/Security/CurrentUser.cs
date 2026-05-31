using Fargo.Application.Identity;

namespace Fargo.ServiceMigration.Security;

/// <summary>
/// Host-safe <see cref="ICurrentUser"/> implementation for the migration worker.
/// </summary>
/// <remarks>
/// The migration worker does not execute in an authenticated request scope,
/// so this implementation returns an anonymous user state.
/// </remarks>
public sealed class CurrentUser : ICurrentUser
{
    /// <inheritdoc />
    public Guid UserGuid => Guid.Empty;

    /// <inheritdoc />
    public bool IsAuthenticated => false;
}
