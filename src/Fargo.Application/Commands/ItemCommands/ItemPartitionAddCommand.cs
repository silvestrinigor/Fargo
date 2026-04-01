using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ItemCommands;

public sealed record ItemAddPartitionCommand(
        Guid ItemGuid,
        Guid PartitionGuid
        ) : ICommand;

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
