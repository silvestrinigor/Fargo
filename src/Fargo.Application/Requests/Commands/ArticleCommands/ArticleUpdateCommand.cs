using Fargo.Application.Dtos.ArticleDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleUpdateCommand(
        Guid ArticleGuid,
        ArticleUpdateDto Article
        ) : ICommand;

    public sealed class ArticleUpdateCommandHandler(
        ArticleService articleService,
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<ArticleUpdateCommand>
    {
        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(ArticleUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleService.GetArticleAsync(command.ArticleGuid, cancellationToken)
                ?? throw new KeyNotFoundException("Article not found.");

            if (command.Article.Name != null)
            {
                article.Name = command.Article.Name.Value;
            }

            if (command.Article.Description != null)
            {
                article.Description = command.Article.Description.Value;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
