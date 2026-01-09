using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemDeleteCommand(Guid ItemGuid) : ICommand;

    public sealed class ItemDeleteCommandHandler(ItemService itemService, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemDeleteCommand>
    {
        private readonly ItemService itemService = itemService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(ItemDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var item = await itemService.GetItemAsync(command.ItemGuid, cancellationToken)
                ?? throw new InvalidOperationException("Item not found.");

            itemService.DeleteItem(item);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
