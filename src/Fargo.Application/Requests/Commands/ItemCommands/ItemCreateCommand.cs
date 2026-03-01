using Fargo.Application.Exceptions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.ArticleServices;
using Fargo.Domain.Services.ItemServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemCreateCommand(
            ItemCreateModel Item
            ) : ICommand<Guid>;

    public sealed class ItemCreateCommandHandler(
            ItemCreateService itemCreateService,
            ArticleGetService articleGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                ItemCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = await articleGetService.GetArticle(
                    actor,
                    command.Item.ArticleGuid,
                    cancellationToken
                    )
                ?? throw new ArticleNotFoundFargoApplicationException(
                        command.Item.ArticleGuid
                        );

            var item = itemCreateService.CreateItem(actor, article);

            await unitOfWork.SaveChanges(cancellationToken);

            return item.Guid;
        }
    }
}