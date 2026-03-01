using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.ItemServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemDeleteCommand(
            Guid ItemGuid
            ) : ICommand;

    public sealed class ItemDeleteCommandHandler(
            ItemDeleteService itemDeleteService,
            ItemGetService itemGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemDeleteCommand>
    {
        public async Task Handle(
                ItemDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var item = await itemGetService.GetItem(
                    actor,
                    command.ItemGuid,
                    cancellationToken
                    )
                ?? throw new ItemNotFoundFargoApplicationException(command.ItemGuid);

            itemDeleteService.DeleteItem(actor, item);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}