using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItensCommands
{
    public sealed record ItemCreateCommand(
        ItemCreateDto Item
        ) : ICommand<Guid>;

    public sealed class ItemCreateCommandHandler(IItemRepository itemRepository, IArticleRepository articleRepository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemCreateCommand, Guid>
    {
        public async Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleRepository.GetByGuidAsync(command.Item.ArticleGuid)
                ?? throw new InvalidOperationException("Article not found.");

            var item = new Item(
                article
                )
            {
                Name = command.Item.Name,
                Description = command.Item.Description,
                ManufacturedAt = command.Item.ManufacturedAt,
            };

            itemRepository.Add(item);

            await unitOfWork.SaveChangesAsync();

            return item.Guid;
        }
    }
}
