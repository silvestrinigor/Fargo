using Fargo.Application.Dtos.ArticlesDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands
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
                command.Article.Description
                );

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return article.Guid;
        }
    }
}