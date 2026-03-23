using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Services;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents an actor corresponding to a real authenticated <see cref="User"/>.
/// </summary>
/// <remarks>
/// This actor is used when an operation is initiated by a real user.
/// <para>
/// The actor's permissions are computed as the union of:
/// <list type="bullet">
/// <item>
/// <description>Permissions directly assigned to the user</description>
/// </item>
/// <item>
/// <description>Permissions inherited from all groups the user belongs to</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// The actor's partition access is derived from:
/// <list type="bullet">
/// <item>
/// <description>Partitions directly assigned to the user</description>
/// </item>
/// <item>
/// <description>Partitions assigned through the user's groups</description>
/// </item>
/// </list>
/// These accesses are typically expanded to include descendant partitions
/// by the <see cref="ActorService"/> before constructing the actor.
/// </para>
/// </remarks>
public sealed class UserActor : Actor
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserActor"/>.
    /// </summary>
    /// <param name="user">
    /// The user associated with the actor.
    /// </param>
    /// <param name="partitionAccesses">
    /// The collection of partition identifiers the actor has access to,
    /// including inherited and expanded (descendant) partitions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is null.
    /// </exception>
    internal UserActor(User user, IReadOnlyCollection<Guid> partitionAccesses)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
        PartitionAccesses = partitionAccesses;
    }

    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public override Guid Guid => User.Guid;

    /// <summary>
    /// Gets the associated <see cref="User"/>.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    /// <remarks>
    /// A user is considered an administrator when its identifier matches
    /// the default administrator defined by <see cref="UserService"/>.
    /// </remarks>
    public override bool IsAdmin => Guid == UserService.DefaultAdministratorUserGuid;

    /// <summary>
    /// Gets a value indicating whether the actor represents a system process.
    /// </summary>
    /// <value>
    /// Always <c>false</c> for <see cref="UserActor"/>.
    /// </value>
    public override bool IsSystem => false;

    /// <summary>
    /// Gets a value indicating whether the underlying user is active.
    /// </summary>
    public override bool IsActive => User.IsActive;

    /// <summary>
    /// Gets the set of permission actions available to the actor.
    /// </summary>
    /// <remarks>
    /// This is computed as the union of permissions directly assigned to the user
    /// and those inherited from all user groups.
    /// </remarks>
    public override IReadOnlyCollection<ActionType> PermissionActions
    {
        get
        {
            var permissions = new HashSet<ActionType>(
                User.Permissions.Select(p => p.Action));

            foreach (var group in User.UserGroups)
            {
                permissions.UnionWith(group.Permissions.Select(p => p.Action));
            }

            return permissions;
        }
    }

    /// <summary>
    /// Gets the collection of partition identifiers the actor has access to.
    /// </summary>
    /// <remarks>
    /// This includes partitions assigned directly to the user and those inherited
    /// from user groups, already expanded to include descendant partitions.
    /// </remarks>
    public override IReadOnlyCollection<Guid> PartitionAccesses { get; }
}
