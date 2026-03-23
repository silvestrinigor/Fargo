using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Services;

namespace Fargo.Domain.Security;

/// <summary>
/// Represents an actor corresponding to a real authenticated <see cref="User"/>.
/// </summary>
/// <remarks>
/// This actor is used when an operation is initiated by a real user.
/// </remarks>
public sealed class UserActor : Actor
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserActor"/>.
    /// </summary>
    /// <param name="user">
    /// The user associated with the actor.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is null.
    /// </exception>
    public UserActor(User user, IReadOnlyCollection<Guid> partitionAccesses)
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

    public override bool IsAdmin => Guid == UserService.DefaultAdministratorUserGuid;

    public override bool IsSystem => false;

    public override IReadOnlyCollection<ActionType> PermissionActions
    {
        get
        {
            var permissions = new HashSet<ActionType>(User.Permissions.Select(p => p.Action));

            foreach (var group in User.UserGroups)
            {
                permissions.UnionWith(group.Permissions.Select(p => p.Action));
            }

            return permissions;
        }
    }

    public override IReadOnlyCollection<Guid> PartitionAccesses { get; }
}
