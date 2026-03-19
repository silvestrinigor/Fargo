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
    /// <remarks>
    /// This value is always equal to <see cref="SystemService.SystemGuid"/>,
    /// which represents the predefined identifier of the internal system actor.
    /// </remarks>
    public Guid Guid { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SystemActor"/>.
    /// </summary>
    /// <remarks>
    /// The created instance will always use the system-defined identifier
    /// (<see cref="SystemService.SystemGuid"/>).
    /// </remarks>
    public SystemActor()
    {
        Guid = SystemService.SystemGuid;
    }
}
