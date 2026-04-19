using Fargo.Application.Partitions;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Command used to create a new <see cref="UserGroup"/>.
/// </summary>
/// <param name="UserGroup">
/// The data required to create the user group, including its identity,
/// optional permissions, and initial partition assignment.
/// </param>
/// <remarks>
/// This command represents the intention to create a group that aggregates
/// permissions and can be assigned to users within a partitioned context.
/// </remarks>
public sealed record UserGroupCreateCommand(
        UserGroupCreateModel UserGroup
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="UserGroupCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Resolving and authorizing the current actor</description></item>
/// <item><description>Validating permission to create user groups</description></item>
/// <item><description>Resolving the target partition (or falling back to global)</description></item>
/// <item><description>Validating access to the target partition</description></item>
/// <item><description>Applying domain validation rules via <see cref="UserGroupService"/></description></item>
/// <item><description>Assigning permissions to the group</description></item>
/// <item><description>Persisting the new user group</description></item>
/// </list>
///
/// Partition behavior:
/// <list type="bullet">
/// <item><description>
/// If <c>FirstPartition</c> is not provided, the group is assigned to the global partition
/// </description></item>
/// <item><description>
/// The actor must have access to the selected partition
/// </description></item>
/// </list>
///
/// Authorization rules:
/// <list type="bullet">
/// <item><description>The actor must have <see cref="ActionType.CreateUserGroup"/> permission</description></item>
/// <item><description>The actor must have access to the target partition</description></item>
/// </list>
///
/// Permission behavior:
/// <list type="bullet">
/// <item><description>
/// Permissions assigned to the group are inherited by users that belong to the group
/// </description></item>
/// <item><description>
/// Permission assignment is explicitly controlled during creation
/// </description></item>
/// </list>
/// </remarks>
public sealed class UserGroupCreateCommandHandler(
        ActorService actorService,
        UserGroupService userGroupService,
        IUserGroupRepository userGroupRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new user group.
    /// </summary>
    /// <param name="command">The command containing the user group creation data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The unique identifier of the created user group.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved or is not authorized.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified or resolved partition does not exist.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to create items.
    /// </exception>
    /// <remarks>
    /// Execution flow:
    /// <list type="number">
    /// <item><description>Resolve the current actor</description></item>
    /// <item><description>Validate <see cref="ActionType.CreateUserGroup"/> permission</description></item>
    /// <item><description>Resolve the target partition (or fallback to global)</description></item>
    /// <item><description>Validate partition access</description></item>
    /// <item><description>Create the user group entity</description></item>
    /// <item><description>Validate domain rules via <see cref="UserGroupService"/></description></item>
    /// <item><description>Assign permissions to the group</description></item>
    /// <item><description>Persist the user group</description></item>
    /// </list>
    /// </remarks>
    public async Task<Guid> Handle(
            UserGroupCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateUserGroup);

        var partitionGuid = command.UserGroup.FirstPartition ?? PartitionService.GlobalPartitionGuid;

        var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        var nameid = ValidateNameid(command.UserGroup.Nameid);

        var userGroup = new UserGroup
        {
            Nameid = nameid,
            Description = command.UserGroup.Description ?? Description.Empty
        };

        userGroup.Partitions.Add(partition);

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        foreach (var permission in command.UserGroup.Permissions ?? [])
        {
            userGroup.AddPermission(permission.Action);
        }

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishUserGroupCreated(userGroup.Guid, userGroup.Nameid, [partition.Guid], cancellationToken);

        return userGroup.Guid;
    }

    private static Nameid ValidateNameid(string value)
    {
        try
        {
            return new Nameid(value);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidNameidFargoApplicationException(ex.Message);
        }
    }
}
