using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands
{
    public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand;

    public sealed class ArticleDeleteCommandHandler(ArticleService articleService, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ArticleDeleteCommand>
    {
        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(ArticleDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleService.GetArticleAsync(command.ArticleGuid, cancellationToken)
                ?? throw new InvalidOperationException("Article not found.");

            await articleService.DeleteArticleAsync(article, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
