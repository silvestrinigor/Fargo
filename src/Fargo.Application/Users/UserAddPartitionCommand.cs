using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.Application.Commands.UserCommands;

/// <summary>
/// Command used to add a partition to a user's partition access set.
/// </summary>
/// <param name="UserGuid">The unique identifier of the user.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to add.</param>
public sealed record UserAddPartitionCommand(
        Guid UserGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="UserAddPartitionCommand"/> requests.
/// </summary>
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
