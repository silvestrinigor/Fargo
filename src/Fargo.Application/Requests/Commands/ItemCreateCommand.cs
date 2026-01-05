using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands
{
    public sealed record ItemCreateCommand(
        ItemCreateDto Item
        ) : ICommand<Guid>;

    public sealed class ItemCreateCommandHandler(IItemRepository itemRepository, IArticleRepository articleRepository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemCreateCommand, Guid>
    {
        private readonly IItemRepository itemRepository = itemRepository;

        private readonly IArticleRepository articleRepository = articleRepository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleRepository.GetByGuidAsync(command.Item.ArticleGuid, cancellationToken)
                ?? throw new InvalidOperationException("Item not found.");

            var item = new Item
            {
                Article = article
            };

            itemRepository.Add(item);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return item.Guid;
        }
    }
}
