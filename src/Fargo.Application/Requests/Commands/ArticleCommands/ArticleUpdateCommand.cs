using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleUpdateCommand(
            Guid ArticleGuid,
            ArticleUpdateModel Article
            ) : ICommand;

    public sealed class ArticleUpdateCommandHandler(
            IArticleRepository articleRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ArticleUpdateCommand>
    {
        public async Task Handle(
                ArticleUpdateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    partitionGuids: null,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = await articleRepository.GetByGuid(
                    command.ArticleGuid,
                    [.. actor.PartitionsAccesses.Select(x => x.Guid)],
                    cancellationToken
                    ) ?? throw new ArticleNotFoundFargoApplicationException(command.ArticleGuid);

            var updated = false;

            if (command.Article.Name is not null)
            {
                article.Name = command.Article.Name.Value;
                updated = true;
            }

            if (command.Article.Description is not null)
            {
                article.Description = command.Article.Description.Value;
                updated = true;
            }

            if (updated)
            {
                article.UpdatedBy = actor;
                article.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}