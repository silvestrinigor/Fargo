using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemUpdateCommand(
        Guid ItemGuid,
        ItemUpdateModel Item
        ) : ICommand;

    public sealed class ItemUpdateCommandHandler(
        ItemService itemService, 
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<ItemUpdateCommand>
    {
        private readonly ItemService itemService = itemService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(ItemUpdateCommand command, CancellationToken cancellationToken = default)
        {
            async Task DefineItemContainer(Guid? containerGuid)
            {
                var item = await itemService.GetItemAsync(command.ItemGuid, cancellationToken);

                if (containerGuid is null)
                {
                    ItemService.RemoveFromContainers(item);
                    return;
                }

                var targetParentItem = await itemService.GetItemAsync(containerGuid.Value, cancellationToken);

                await itemService.InsertItemIntoContainerAsync(item, targetParentItem);
            }

            if (command.Item.ParentItemGuid is not null)
            {
                await DefineItemContainer(command.Item.ParentItemGuid.Value);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
