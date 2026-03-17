using Fargo.Application.Security;
using Fargo.Domain.Security;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Represents a system-level implementation of <see cref="ICurrentUser"/>.
/// </summary>
/// <remarks>
/// This implementation is used when an operation is executed by the
/// application itself rather than by an authenticated human user.
///
/// Typical scenarios include:
/// <list type="bullet">
/// <item>
/// <description>background workers</description>
/// </item>
/// <item>
/// <description>scheduled or automated processes</description>
/// </item>
/// <item>
/// <description>database maintenance tasks</description>
/// </item>
/// <item>
/// <description>system-triggered domain operations</description>
/// </item>
/// </list>
///
/// In these cases, the actor responsible for the operation is represented
/// by the predefined <see cref="SystemActor.Guid"/> identifier.
/// </remarks>
public sealed class SystemCurrentUser : ICurrentUser
{
    /// <summary>
    /// Gets a value indicating whether the current actor is authenticated.
    /// </summary>
    /// <remarks>
    /// System operations are always considered authenticated because
    /// they are executed internally by the application.
    /// </remarks>
    public bool IsAuthenticated
        => true;

    /// <summary>
    /// Gets the unique identifier representing the system actor.
    /// </summary>
    /// <remarks>
    /// This value always returns <see cref="SystemActor.Guid"/>,
    /// which is the predefined identifier used to represent the
    /// internal system actor during automated operations.
    /// </remarks>
    public Guid UserGuid => SystemActor.Guid;
}
