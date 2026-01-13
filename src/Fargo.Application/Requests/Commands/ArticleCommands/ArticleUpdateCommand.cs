using Fargo.Application.Mediators;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleUpdateCommand(
        Guid ArticleGuid,
        ArticleUpdateModel Article
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
            var article = await articleService.GetArticleAsync(command.ArticleGuid, cancellationToken);

            if (command.Article.Name is not null)
            {
                article.Name = command.Article.Name.Value;
            }

            if (command.Article.Description is not null)
            {
                article.Description = command.Article.Description.Value;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
