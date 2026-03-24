using Fargo.Domain.Enums;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents an actor responsible for performing operations within the system.
/// </summary>
/// <remarks>
/// An actor abstracts the origin of an action, allowing the domain to treat
/// different initiators uniformly. An actor can be:
/// <list type="bullet">
/// <item>
/// <description>A real authenticated user (<see cref="UserActor"/>)</description>
/// </item>
/// <item>
/// <description>The system itself (<see cref="SystemActor"/>)</description>
/// </item>
/// </list>
///
/// This abstraction enables consistent authorization, auditing, and permission handling
/// across the domain, regardless of whether the action was triggered by a user or internally.
/// </remarks>
public interface IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies the actor instance.
    /// </value>
    Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is an administrator; otherwise, <c>false</c>.
    /// </value>
    bool IsAdmin { get; }

    /// <summary>
    /// Gets a value indicating whether the actor represents the system.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is the system actor; otherwise, <c>false</c>.
    /// </value>
    bool IsSystem { get; }

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is active and allowed to perform actions; otherwise, <c>false</c>.
    /// </value>
    bool IsActive { get; }

    /// <summary>
    /// Gets the set of actions the actor is permitted to perform.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="ActionType"/> representing allowed operations.
    /// </value>
    IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>
    /// Gets the set of partitions the actor has access to.
    /// </summary>
    /// <value>
    /// A read-only collection of partition identifiers (<see cref="Guid"/>).
    /// </value>
    IReadOnlyCollection<Guid> PartitionAccesses { get; }

    /// <summary>
    /// Determines whether the actor has access to a specific partition.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <returns>
    /// <c>true</c> if the actor has access to the specified partition; otherwise, <c>false</c>.
    /// </returns>
    bool HasPartitionAccess(Guid partitionGuid);

    /// <summary>
    /// Determines whether the actor has permission to perform a specific action.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor has permission for the specified action; otherwise, <c>false</c>.
    /// </returns>
    bool HasActionPermission(ActionType action);
}
