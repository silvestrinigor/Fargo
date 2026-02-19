using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemDeleteCommand(
            Guid ItemGuid
            ) : ICommand;

    public sealed class ItemDeleteCommandHandler(
        ItemService itemService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemDeleteCommand>
    {
        public async Task Handle(
                ItemDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var item = await itemService.GetItem(
                    actor,
                    command.ItemGuid,
                    cancellationToken
                    )
                ?? throw new ItemNotFoundFargoApplicationException(
                        command.ItemGuid
                        );

            itemService.DeleteItem(actor, item);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}