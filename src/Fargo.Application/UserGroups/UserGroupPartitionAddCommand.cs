using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.Application.Commands.UserGroupCommands;

/// <summary>
/// Command used to add a partition to a user group's partition access set.
/// </summary>
/// <param name="UserGroupGuid">The unique identifier of the user group.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to add.</param>
public sealed record UserGroupPartitionAddCommand(
        Guid UserGroupGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="UserGroupPartitionAddCommand"/> requests.
/// </summary>
public sealed class UserGroupPartitionAddCommandHandler(
        ActorService actorService,
        IUserGroupRepository userGroupRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserGroupPartitionAddCommand>
{
    public async Task Handle(
            UserGroupPartitionAddCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (!userGroup.Partitions.Contains(partition))
        {
            userGroup.Partitions.Add(partition);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
