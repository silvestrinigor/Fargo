using Fargo.Domain.Enums;
using Fargo.Domain.Services;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents the internal system actor.
/// </summary>
/// <remarks>
/// This actor is used when an operation is performed by the system
/// itself rather than by a real authenticated user.
/// </remarks>
public sealed class SystemActor : Actor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    /// <remarks>
    /// This value is always equal to <see cref="SystemService.SystemGuid"/>,
    /// which represents the predefined identifier of the internal system actor.
    /// </remarks>
    public override Guid Guid { get; }

    public override bool IsAdmin => true;

    public override bool IsSystem => true;

    public override IReadOnlyCollection<ActionType> PermissionActions => [];

    public override IReadOnlyCollection<Guid> PartitionAccesses => [];

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
