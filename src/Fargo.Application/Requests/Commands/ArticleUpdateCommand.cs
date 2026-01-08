using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands
{
    public sealed record ArticleUpdateCommand(
        Guid ArticleGuid,
        ArticleUpdateDto Article
        ) : ICommand;

    public sealed class ArticleUpdateCommandHandler(
        ArticleService articleService
        ) : ICommandHandlerAsync<ArticleUpdateCommand>
    {
        private readonly ArticleService articleService = articleService;

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
        }
    }
}
