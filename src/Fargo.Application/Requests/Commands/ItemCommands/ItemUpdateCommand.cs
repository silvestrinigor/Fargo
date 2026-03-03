using Fargo.Application.Exceptions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemUpdateCommand(
            Guid ItemGuid,
            ItemUpdateModel Item
            ) : ICommand;

    public sealed class ItemUpdateCommandHandler(
            IItemRepository itemRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemUpdateCommand>
    {
        public async Task Handle(ItemUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    partitionGuids: null,
                    cancellationToken
                    )
                ?? throw new UnauthorizedAccessFargoApplicationException();

            var actorPartitionGuids = actor.PartitionsAccesses.Select(x => x.Guid).ToList();

            async Task DefineItemContainer(Guid? containerGuid)
            {
                var item = await itemRepository.GetByGuid(
                        command.ItemGuid,
                        actorPartitionGuids,
                        cancellationToken
                        )
                    ?? throw new ItemNotFoundFargoApplicationException(
                            command.ItemGuid
                            );

                if (containerGuid is null)
                {
                    //ItemService.RemoveFromContainers(item);
                    return;
                }

                var targetParentItem = await itemRepository.GetByGuid(
                        containerGuid.Value,
                        actorPartitionGuids,
                        cancellationToken
                        )
                    ?? throw new ItemNotFoundFargoApplicationException(containerGuid.Value);

                //await itemService.InsertItemIntoContainerAsync(item, targetParentItem);
            }

            if (command.Item.ParentItemGuid is not null)
            {
                await DefineItemContainer(command.Item.ParentItemGuid.Value);
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}