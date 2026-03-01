using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.ArticleServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleUpdateCommand(
            Guid ArticleGuid,
            ArticleUpdateModel Article
            ) : ICommand;

    public sealed class ArticleUpdateCommandHandler(
            ArticleGetService articleGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ArticleUpdateCommand>
    {
        public async Task Handle(
                ArticleUpdateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = await articleGetService.GetArticle(
                    actor,
                    command.ArticleGuid,
                    cancellationToken
                    ) ?? throw new ArticleNotFoundFargoApplicationException(command.ArticleGuid);

            if (command.Article.Name is not null)
            {
                article.Name = command.Article.Name.Value;
            }

            if (command.Article.Description is not null)
            {
                article.Description = command.Article.Description.Value;
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}