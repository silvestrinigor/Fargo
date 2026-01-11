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
                ?? throw new InvalidOperationException("Article not found.");

            article.Name = command.Article.Name is not null ? command.Article.Name.Value : article.Name;

            article.Description = command.Article.Description is not null ? command.Article.Description.Value : article.Description;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
