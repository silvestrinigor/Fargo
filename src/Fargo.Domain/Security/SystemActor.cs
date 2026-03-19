using Fargo.Domain.Services;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents the internal system actor.
/// </summary>
/// <remarks>
/// This actor is used when an operation is performed by the system
/// itself rather than by a real authenticated user.
/// </remarks>
public sealed class SystemActor : IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SystemActor"/>.
    /// </summary>
    /// <param name="systemService">
    /// The service that provides the default system identity.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="systemService"/> is <see langword="null"/>.
    /// </exception>
    public SystemActor(SystemService systemService)
    {
        ArgumentNullException.ThrowIfNull(systemService);

        Guid = systemService.SystemGuid;
    }
}
