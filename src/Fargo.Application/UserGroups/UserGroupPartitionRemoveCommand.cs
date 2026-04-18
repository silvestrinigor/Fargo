using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Commands.UserGroupCommands;

/// <summary>
/// Command used to remove a partition from a user group's partition access set.
/// </summary>
/// <param name="UserGroupGuid">The unique identifier of the user group.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to remove.</param>
public sealed record UserGroupPartitionRemoveCommand(
        Guid UserGroupGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="UserGroupPartitionRemoveCommand"/> requests.
/// </summary>
public sealed class UserGroupPartitionRemoveCommandHandler(
        ActorService actorService,
        IUserGroupRepository userGroupRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserGroupPartitionRemoveCommand>
{
    public async Task Handle(
            UserGroupPartitionRemoveCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        var partitionToRemove = userGroup.Partitions.FirstOrDefault(p => p.Guid == command.PartitionGuid);

        if (partitionToRemove is not null)
        {
            actor.ValidateHasPartitionAccess(partitionToRemove.Guid);

            userGroup.Partitions.Remove(partitionToRemove);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
