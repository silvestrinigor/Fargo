using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ItemCommands;

/// <summary>
/// Command used to remove a partition from an item.
/// </summary>
/// <param name="ItemGuid">The unique identifier of the item.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to remove.</param>
public sealed record ItemRemovePartitionCommand(
        Guid ItemGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="ItemRemovePartitionCommand"/> requests.
/// </summary>
public sealed class ItemRemovePartitionCommandHandler(
        ActorService actorService,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemRemovePartitionCommand>
{
    public async Task Handle(
            ItemRemovePartitionCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        var partitionToRemove = item.Partitions.FirstOrDefault(p => p.Guid == command.PartitionGuid);

        if (partitionToRemove is not null)
        {
            actor.ValidateHasPartitionAccess(partitionToRemove.Guid);

            item.Partitions.Remove(partitionToRemove);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
