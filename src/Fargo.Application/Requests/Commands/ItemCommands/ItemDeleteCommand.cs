using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemDeleteCommand(Guid ItemGuid) : ICommand<Task>;

    public sealed class ItemDeleteCommandHandler(
        ItemService itemService,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<ItemDeleteCommand, Task>
    {
        private readonly ItemService itemService = itemService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(ItemDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var item = await itemService.GetItemAsync(command.ItemGuid, cancellationToken);

            itemService.DeleteItem(item);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}