using Fargo.Application.Extensions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemUpdateCommand(
        Guid ItemGuid,
        ItemUpdateModel Item
        ) : ICommand<Task>;

    public sealed class ItemUpdateCommandHandler(
        ItemService itemService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemUpdateCommand, Task>
    {
        private readonly ItemService itemService = itemService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task Handle(ItemUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var actor = currentUser.ToActor();

            async Task DefineItemContainer(Guid? containerGuid)
            {
                var item = await itemService.GetItemAsync(actor, command.ItemGuid, cancellationToken);

                if (containerGuid is null)
                {
                    ItemService.RemoveFromContainers(item);
                    return;
                }

                var targetParentItem = await itemService.GetItemAsync(
                        actor,
                        containerGuid.Value,
                        cancellationToken
                        );

                await itemService.InsertItemIntoContainerAsync(item, targetParentItem);
            }

            if (command.Item.ParentItemGuid is not null)
            {
                await DefineItemContainer(command.Item.ParentItemGuid.Value);
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}