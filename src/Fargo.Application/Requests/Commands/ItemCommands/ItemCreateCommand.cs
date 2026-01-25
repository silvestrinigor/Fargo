using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemCreateCommand(
        ItemCreateModel Item
        ) : ICommand<Task<Guid>>;

    public sealed class ItemCreateCommandHandler(
        ItemService itemService,
        ArticleService articleService,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<ItemCreateCommand, Task<Guid>>
    {
        private readonly ItemService itemService = itemService;

        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> Handle(ItemCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleService.GetArticleAsync(command.Item.ArticleGuid, cancellationToken);

            var item = itemService.CreateItem(article);

            await unitOfWork.SaveChanges(cancellationToken);

            return item.Guid;
        }
    }
}