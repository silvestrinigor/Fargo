using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands
{
    public sealed record ItemDeleteCommand(Guid ItemGuid) : ICommand;

    public sealed class ItemDeleteCommandHandler(IItemRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemDeleteCommand>
    {
        public async Task HandleAsync(ItemDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var item = await repository.GetByGuidAsync(command.ItemGuid, cancellationToken)
                ?? throw new InvalidOperationException("Item not found.");

            repository.Remove(item);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
