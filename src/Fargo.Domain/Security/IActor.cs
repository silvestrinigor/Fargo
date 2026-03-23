using Fargo.Domain.Enums;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents an actor responsible for performing an operation in the system.
/// </summary>
/// <remarks>
/// An actor can be:
/// - a real user (<see cref="UserActor"/>)
/// - the system itself (<see cref="SystemActor"/>)
///
/// This abstraction allows the domain to treat all action initiators uniformly.
/// </remarks>
public interface IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    Guid Guid { get; }

    bool IsAdmin { get; }

    bool IsSystem { get; }

    IReadOnlyCollection<ActionType> PermissionActions { get; }

    IReadOnlyCollection<Guid> PartitionAccesses { get; }

    bool HasPartitionAccess(Guid partitionGuid);

    bool HasActionPermission(ActionType action);
}
