using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.UserGroupCommands;

public sealed record UserGroupPartitionRemoveCommand(
        Guid UserGroupGuid,
        Guid PartitionGuid
        ) : ICommand;

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
