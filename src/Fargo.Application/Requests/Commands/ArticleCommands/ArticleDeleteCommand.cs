using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand<Task>;

    public sealed class ArticleDeleteCommandHandler(
        ArticleService articleService,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<ArticleDeleteCommand, Task>
    {
        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(ArticleDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var article = await articleService.GetArticleAsync(command.ArticleGuid, cancellationToken);

            await articleService.DeleteArticleAsync(article, cancellationToken);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}
