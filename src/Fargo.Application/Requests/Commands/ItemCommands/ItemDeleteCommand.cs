using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemDeleteCommand(Guid ItemGuid) : ICommand<Task>;

    public sealed class ItemDeleteCommandHandler(
        ItemService itemService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemDeleteCommand, Task>
    {
        private readonly ItemService itemService = itemService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task Handle(
                ItemDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var item = await itemService.GetItemAsync(
                    actor,
                    command.ItemGuid,
                    cancellationToken
                    );

            itemService.DeleteItem(actor, item);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}