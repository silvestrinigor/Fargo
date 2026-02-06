using Fargo.Application.Extensions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemCreateCommand(
        ItemCreateModel Item
        ) : ICommand<Task<Guid>>;

    public sealed class ItemCreateCommandHandler(
        ItemService itemService,
        ArticleService articleService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemCreateCommand, Task<Guid>>
    {
        private readonly ItemService itemService = itemService;

        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task<Guid> Handle(
                ItemCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var article = await articleService.GetArticleAsync(
                    actor,
                    command.Item.ArticleGuid,
                    cancellationToken
                    );

            var item = itemService.CreateItem(actor, article);

            await unitOfWork.SaveChanges(cancellationToken);

            return item.Guid;
        }
    }
}