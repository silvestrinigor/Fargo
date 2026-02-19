using Fargo.Application.Extensions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
            ArticleCreateModel Article
            ) : ICommand<Guid>;

    public sealed class ArticleCreateCommandHandler(
            ArticleService service,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ArticleCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                ArticleCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var article = service.CreateArticle(
                    actor,
                    command.Article.Name,
                    command.Article.Description ?? default,
                    command.Article.IsContainer
                    );

            await unitOfWork.SaveChanges(cancellationToken);

            return article.Guid;
        }
    }
}