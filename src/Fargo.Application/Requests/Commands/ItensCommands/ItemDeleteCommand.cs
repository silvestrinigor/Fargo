using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItensCommands
{
    public sealed record ItemDeleteCommand(Guid ItemGuid) : ICommand;

    public sealed class ArticleDeleteCommandHandler(IItemRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemDeleteCommand>
    {
        public async Task HandleAsync(ItemDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var item = await repository.GetByGuidAsync(command.ItemGuid)
                ?? throw new InvalidOperationException("Item not found.");

            repository.Remove(item);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
