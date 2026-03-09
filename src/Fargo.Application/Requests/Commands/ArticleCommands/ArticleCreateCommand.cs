using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    /// <summary>
    /// Command used to create a new <see cref="Article"/>.
    /// </summary>
    /// <param name="Article">
    /// The data required to create the article.
    /// </param>
    public sealed record ArticleCreateCommand(
            ArticleCreateModel Article
            ) : ICommand<Guid>;

    /// <summary>
    /// Handles the execution of <see cref="ArticleCreateCommand"/>.
    /// </summary>
    public sealed class ArticleCreateCommandHandler(
            IArticleRepository articleRepository,
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<ArticleCreateCommand, Guid>
    {
        /// <summary>
        /// Executes the command to create a new article.
        /// </summary>
        /// <param name="command">The command containing article creation data.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The unique identifier of the created article.</returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the current user cannot be resolved.
        /// </exception>
        public async Task<Guid> Handle(
                ArticleCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            actor.ValidatePermission(ActionType.CreateArticle);

            var article = new Article
            {
                Name = command.Article.Name,
                Description = command.Article.Description ?? Description.Empty
            };

            articleRepository.Add(article);

            await unitOfWork.SaveChanges(cancellationToken);

            return article.Guid;
        }
    }
}