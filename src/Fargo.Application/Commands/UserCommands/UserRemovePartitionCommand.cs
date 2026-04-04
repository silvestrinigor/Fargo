using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.UserCommands;

/// <summary>
/// Command used to remove a partition from a user's partition access set.
/// </summary>
/// <param name="UserGuid">The unique identifier of the user.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to remove.</param>
public sealed record UserRemovePartitionCommand(
        Guid UserGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="UserRemovePartitionCommand"/> requests.
/// </summary>
public sealed class UserRemovePartitionCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserRemovePartitionCommand>
{
    public async Task Handle(
            UserRemovePartitionCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        var partitionToRemove = user.Partitions.FirstOrDefault(p => p.Guid == command.PartitionGuid);

        if (partitionToRemove is not null)
        {
            actor.ValidateHasPartitionAccess(partitionToRemove.Guid);

            user.Partitions.Remove(partitionToRemove);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
