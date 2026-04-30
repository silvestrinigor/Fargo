using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Events;
using Fargo.Domain.Users;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Command used to delete an existing user group.
/// </summary>
/// <param name="UserGroupGuid">
/// The unique identifier of the user group to delete.
/// </param>
public sealed record UserGroupDeleteCommand(
        Guid UserGroupGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="UserGroupDeleteCommand"/>.
/// </summary>
public sealed class UserGroupDeleteCommandHandler(
        ActorService actorService,
        IUserGroupRepository userGroupRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IEventRecorder eventRecorder,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<UserGroupDeleteCommand>
{
    /// <summary>
    /// Executes the command to delete an existing user group.
    /// </summary>
    /// <param name="command">
    /// The command containing the user group identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="UserGroupNotFoundFargoApplicationException">
    /// Thrown when the specified user group does not exist.
    /// </exception>
    public async Task Handle(
            UserGroupDeleteCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        UserGroupService.ValidateUserGroupDelete(userGroup, actor);

        userGroupRepository.Remove(userGroup);

        await eventRecorder.Record(EventType.UserGroupDeleted, EntityType.UserGroup, userGroup.Guid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        await eventPublisher.PublishUserGroupDeleted(userGroup.Guid, cancellationToken);
    }
}
