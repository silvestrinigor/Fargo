using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands
{
    public sealed record ItemCreateCommand(
        ItemCreateDto Item
        ) : ICommand<Guid>;

    public sealed class ItemCreateCommandHandler(ItemService itemService, ArticleService articleService, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ItemCreateCommand, Guid>
    {
        private readonly ItemService itemService = itemService;

        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleService.GetArticleAsync(command.Item.ArticleGuid, cancellationToken)
                ?? throw new InvalidOperationException("Article not found.");

            var item = itemService.CreateItem(article);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return item.Guid;
        }
    }
}