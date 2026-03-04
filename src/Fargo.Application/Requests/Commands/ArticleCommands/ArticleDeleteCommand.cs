using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleDeleteCommand(
            Guid ArticleGuid
            ) : ICommand;

    public sealed class ArticleDeleteCommandHandler(
            IArticleRepository articleRepository,
            IUserRepository userRepository,
            ArticleService articleService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ArticleDeleteCommand>
    {
        public async Task Handle(
                ArticleDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = await articleRepository.GetByGuid(
                    command.ArticleGuid,
                    cancellationToken
                    ) ?? throw new ArticleNotFoundFargoApplicationException(command.ArticleGuid);

            actor.ValidatePermission(ActionType.DeleteArticle);

            await articleService.ValidateArticleDelete(article, cancellationToken);

            articleRepository.Remove(article);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}