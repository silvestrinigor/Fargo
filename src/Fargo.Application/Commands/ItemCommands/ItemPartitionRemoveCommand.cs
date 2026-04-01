using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ItemCommands;

public sealed record ItemRemovePartitionCommand(
        Guid ItemGuid,
        Guid PartitionGuid
        ) : ICommand;

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
