using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.UserCommands;

public sealed record UserAddPartitionCommand(
        Guid UserGuid,
        Guid PartitionGuid
        ) : ICommand;

public sealed class UserAddPartitionCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserAddPartitionCommand>
{
    public async Task Handle(
            UserAddPartitionCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (!user.Partitions.Contains(partition))
        {
            user.Partitions.Add(partition);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
