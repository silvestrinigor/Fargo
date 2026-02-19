using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemCreateCommand(
        ItemCreateModel Item
        ) : ICommand<Guid>;

    public sealed class ItemCreateCommandHandler(
        ItemService itemService,
        ArticleService articleService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                ItemCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var article = await articleService.GetArticle(
                    actor,
                    command.Item.ArticleGuid,
                    cancellationToken
                    )
                ?? throw new ArticleNotFoundFargoApplicationException(
                        command.Item.ArticleGuid
                        );

            var item = itemService.CreateItem(actor, article);

            await unitOfWork.SaveChanges(cancellationToken);

            return item.Guid;
        }
    }
}