using Fargo.Application.Dtos.ArticleDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(ArticleCreateDto Article) : ICommand<Guid>;

    public sealed class ArticleCreateCommandHandler(ArticleService articleService, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ArticleCreateCommand, Guid>
    {
        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(ArticleCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = articleService.CreateArticle(
                command.Article.Name,
                command.Article.Description,
                command.Article.IsContainer
                );

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return article.Guid;
        }
    }
}