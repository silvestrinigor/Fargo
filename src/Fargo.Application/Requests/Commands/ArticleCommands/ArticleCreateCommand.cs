using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
            ArticleCreateModel Article
            ) : ICommand<Guid>;

    public sealed class ArticleCreateCommandHandler(
            IArticleRepository articleRepository,
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<ArticleCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                ArticleCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var userActor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    null,
                    cancellationToken)
                ?? throw new UserNotFoundFargoApplicationException(currentUser.UserGuid);

            if(!PermissionService.HasPermission(
                        userActor, ActionType.CreateArticle))
                throw new UserNotHavePermissionFargoApplicationException(
                        userActor.Guid, ActionType.CreateArticle);

            var article = new Article
            {
                Name = command.Article.Name,
                Description = command.Article.Description ?? default,
                IsContainer = command.Article.IsContainer
            };

            articleRepository.Add(article);

            await unitOfWork.SaveChanges(cancellationToken);

            return article.Guid;
        }
    }
}