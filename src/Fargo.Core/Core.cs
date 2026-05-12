using Fargo.Core.Partitions;
using Fargo.Core.System;
using Fargo.Core.Users;

namespace Fargo.Core;

/// <summary>
/// Service responsible for resolving and constructing <see cref="Actor"/> instances,
/// including <see cref="UserActor"/> and <see cref="SystemActor"/>.
/// </summary>
/// <remarks>
/// This service centralizes the logic for loading users and aggregating their
/// effective partition access based on direct assignments and group memberships.
/// </remarks>

public class ActorService(
    IUserRepository userRepository,
    IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Retrieves an <see cref="Actor"/> instance based on its unique identifier.
    /// </summary>
    /// <param name="actorGuid">The unique identifier of the actor.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="System.SystemActor"/> if the identifier matches the system actor;
    /// a <see cref="UserActor"/> if a corresponding user is found;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// For user actors, this method resolves effective partition access by:
    /// <list type="bullet">
    /// <item><description>Including partitions directly assigned to the user</description></item>
    /// <item><description>Including partitions inherited through user group memberships</description></item>
    /// <item><description>Expanding all collected partitions to include their descendants</description></item>
    /// </list>
    /// This ensures the returned actor contains the complete set of accessible partitions.
    /// </remarks>
    public async Task<Actor?> GetActorByGuid(
        Guid actorGuid,
        CancellationToken cancellationToken = default)
    {
        if (actorGuid == SystemService.SystemGuid)
        {
            var allPartitionAccesses = await partitionRepository.GetDescendantGuids(
                PartitionService.GlobalPartitionGuid,
                includeRoot: true,
                cancellationToken);

            var systemActor = new SystemActor(
                Enum.GetValues<ActionType>(),
                allPartitionAccesses);

            return systemActor;
        }

        var user = await userRepository.GetByGuid(actorGuid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var parentPartitions = user.PartitionAccesses
            .Select(p => p.PartitionGuid)
            .ToHashSet();

        parentPartitions.UnionWith(
            user.UserGroups
                .Where(group => group.IsActive)
                .SelectMany(g => g.PartitionAccesses)
                .Select(p => p.PartitionGuid)
        );

        var partitionAccess = await partitionRepository
            .GetDescendantGuids(parentPartitions, true, cancellationToken);

        return new UserActor(user, partitionAccess);
    }
}

/// <summary>
/// Represents the set of actions that can be authorized in the system.
///
/// Each value defines a specific permission that can be granted
/// to a user through <see cref="Fargo.Core.Users.UserPermission"/>.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Allows creating new articles.
    /// </summary>
    CreateArticle = 0,

    /// <summary>
    /// Allows deleting existing articles.
    /// </summary>
    DeleteArticle = 1,

    /// <summary>
    /// Allows editing existing articles.
    /// </summary>
    EditArticle = 2,

    /// <summary>
    /// Allows creating new items associated with articles.
    /// </summary>
    CreateItem = 3,

    /// <summary>
    /// Allows deleting existing items.
    /// </summary>
    DeleteItem = 4,

    /// <summary>
    /// Allows editing existing items.
    /// </summary>
    EditItem = 5,

    /// <summary>
    /// Allows creating new users.
    /// </summary>
    CreateUser = 6,

    /// <summary>
    /// Allows deleting existing users.
    /// </summary>
    DeleteUser = 7,

    /// <summary>
    /// Allows editing existing users.
    /// </summary>
    EditUser = 8,

    /// <summary>
    /// Allows changing the password of another user without requiring
    /// the current password of that user.
    ///
    /// This permission is typically intended for administrative users
    /// who need to reset another user's password. It does not apply to
    /// a user changing their own password, which should require the
    /// current password.
    /// </summary>
    ChangeOtherUserPassword = 9,

    /// <summary>
    /// Allows creating new user groups.
    /// </summary>
    CreateUserGroup = 10,

    /// <summary>
    /// Allows deleting existing user groups.
    /// </summary>
    DeleteUserGroup = 11,

    /// <summary>
    /// Allows editing existing user groups.
    /// </summary>
    EditUserGroup = 12,

    /// <summary>
    /// Allows modifying the membership of a user group,
    /// including adding or removing users from the group.
    ///
    /// This permission controls operations that change which
    /// users belong to a given <see cref="Fargo.Core.UserGroups.UserGroup"/>.
    /// It does not grant permission to create, delete, or edit the group
    /// itself, which are controlled by <see cref="CreateUserGroup"/>,
    /// <see cref="DeleteUserGroup"/>, and <see cref="EditUserGroup"/>.
    /// </summary>
    ChangeUserGroupMembers = 13,

    /// <summary>
    /// Allows creating partitions.
    /// </summary>
    CreatePartition = 14,

    /// <summary>
    /// Allows deleting existing partitions.
    /// </summary>
    DeletePartition = 15,

    /// <summary>
    /// Allows editing existing partitions.
    /// </summary>
    EditPartition = 16,

    /// <summary>
    /// Allows adding barcodes to articles.
    /// </summary>
    AddBarcode = 17,

    /// <summary>
    /// Allows removing barcodes from articles.
    /// </summary>
    RemoveBarcode = 18,

}
