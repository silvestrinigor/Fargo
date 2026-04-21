using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Items;

/// <summary>
/// Command used to add a partition to an item.
/// </summary>
/// <param name="ItemGuid">The unique identifier of the item.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to add.</param>
public sealed record ItemAddPartitionCommand(
        Guid ItemGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="ItemAddPartitionCommand"/> requests.
/// </summary>
public sealed class ItemAddPartitionCommandHandler(
        ActorService actorService,
        IItemRepository itemRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemAddPartitionCommand>
{
    public async Task Handle(
            ItemAddPartitionCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (!item.Partitions.Contains(partition))
        {
            item.Partitions.Add(partition);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
