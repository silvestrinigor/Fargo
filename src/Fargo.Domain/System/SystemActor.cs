namespace Fargo.Domain.System;

/// <summary>
/// Represents the internal system actor.
/// </summary>
/// <remarks>
/// This actor is used when an operation is performed by the system
/// itself rather than by a real authenticated user.
/// <para>
/// The system actor has full access to all operations and partitions,
/// typically bypassing standard authorization checks.
/// </para>
/// </remarks>
public sealed class SystemActor : Actor
{
    /// <summary>
    /// Initializes a new instance of <see cref="SystemActor"/>.
    /// </summary>
    /// <remarks>
    /// The created instance will always use the system-defined identifier
    /// (<see cref="SystemService.SystemGuid"/>).
    /// </remarks>
    internal SystemActor()
    {
        Guid = SystemService.SystemGuid;
    }

    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    /// <remarks>
    /// This value is always equal to <see cref="SystemService.SystemGuid"/>,
    /// which represents the predefined identifier of the internal system actor.
    /// </remarks>
    public override Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    /// <value>
    /// Always <c>true</c> for <see cref="SystemActor"/>.
    /// </value>
    /// <remarks>
    /// The system actor is considered an administrator and is typically
    /// allowed to bypass permission and partition checks.
    /// </remarks>
    public override bool IsAdmin => true;

    /// <summary>
    /// Gets a value indicating whether the actor represents the system.
    /// </summary>
    /// <value>
    /// Always <c>true</c>.
    /// </value>
    public override bool IsSystem => true;

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    /// <value>
    /// Always <c>true</c>.
    /// </value>
    public override bool IsActive => true;

    /// <summary>
    /// Gets the set of permission actions available to the actor.
    /// </summary>
    /// <value>
    /// An empty collection.
    /// </value>
    /// <remarks>
    /// The system actor does not rely on explicit permission entries.
    /// Authorization is granted implicitly through <see cref="IsAdmin"/>.
    /// </remarks>
    public override IReadOnlyCollection<ActionType> PermissionActions => [];

    /// <summary>
    /// Gets the collection of partition identifiers the actor has access to.
    /// </summary>
    /// <value>
    /// An empty collection.
    /// </value>
    /// <remarks>
    /// The system actor is not restricted by partitions. Partition checks
    /// are expected to be bypassed due to <see cref="IsAdmin"/>.
    /// </remarks>
    public override IReadOnlyCollection<Guid> PartitionAccesses => [];
}
