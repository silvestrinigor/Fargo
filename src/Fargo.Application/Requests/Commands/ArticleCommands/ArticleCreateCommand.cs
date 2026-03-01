using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.ArticleServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
            ArticleCreateModel Article
            ) : ICommand<Guid>;

    public sealed class ArticleCreateCommandHandler(
            ArticleCreateService articleCreateService,
            ActorGetService actorGetService,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<ArticleCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                ArticleCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = articleCreateService.CreateArticle(
                    actor,
                    command.Article.Name,
                    command.Article.IsContainer
                    );

            await unitOfWork.SaveChanges(cancellationToken);

            return article.Guid;
        }
    }
}