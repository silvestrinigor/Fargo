using Fargo.Application.Exceptions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.ItemServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemUpdateCommand(
            Guid ItemGuid,
            ItemUpdateModel Item
            ) : ICommand;

    public sealed class ItemUpdateCommandHandler(
            ItemGetService itemGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemUpdateCommand>
    {
        public async Task Handle(ItemUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            async Task DefineItemContainer(Guid? containerGuid)
            {
                var item = await itemGetService.GetItem(
                        actor,
                        command.ItemGuid,
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

                var targetParentItem = await itemGetService.GetItem(
                        actor,
                        containerGuid.Value,
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