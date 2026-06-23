using Fargo.Application.Identity;

namespace Fargo.ServiceSeed.Security;

/// <summary>
/// Host-safe <see cref="ICurrentActor"/> implementation for the seed worker.
/// </summary>
/// <remarks>
/// The seed worker does not run under an authenticated HTTP request, so it
/// exposes an anonymous current user to satisfy application-layer dependencies.
/// </remarks>
public sealed class CurrentUser : ICurrentActor
{
    /// <inheritdoc />
    public Guid UserGuid => Guid.Empty;

    /// <inheritdoc />
    public bool IsAuthenticated => false;
}
