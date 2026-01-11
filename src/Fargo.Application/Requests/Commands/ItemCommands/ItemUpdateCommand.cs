using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemUpdateCommand(
        Guid ItemGuid,
        ItemUpdateDto Item
        ) : ICommand;

    public sealed class ItemUpdateCommandHandler(ItemService itemService, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemUpdateCommand>
    {
        private readonly ItemService itemService = itemService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(ItemUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var item = await itemService.GetItemAsync(command.ItemGuid, cancellationToken)
                ?? throw new InvalidOperationException("Item not found.");

            if (command.Item.ParentItemGuid.SetValue)
            {
                if (command.Item.ParentItemGuid.Value is null)
                {
                    ItemService.RemoveFromContainers(item);
                }
                else
                {
                    var targetParentItem = await itemService.GetItemAsync(command.Item.ParentItemGuid.Value.Value, cancellationToken)
                        ?? throw new InvalidOperationException("Target parent item not found.");

                    await itemService.InsertItemIntoContainerAsync(item, targetParentItem);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
