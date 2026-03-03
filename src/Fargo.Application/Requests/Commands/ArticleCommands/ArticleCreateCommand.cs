using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

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
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    partitionGuids: null,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            actor.ValidatePermission(ActionType.CreateArticle);

            var article = new Article
            {
                Name = command.Article.Name,
                IsContainer = command.Article.IsContainer,
                UpdatedBy = actor
            };

            articleRepository.Add(article);

            await unitOfWork.SaveChanges(cancellationToken);

            return article.Guid;
        }
    }
}