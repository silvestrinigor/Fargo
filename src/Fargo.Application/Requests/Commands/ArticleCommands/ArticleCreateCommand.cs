using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
        ArticleCreateModel Article
        ) : ICommand<Task<Guid>>;

    public sealed class ArticleCreateCommandHandler(
        ArticleService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<ArticleCreateCommand, Task<Guid>>
    {
        private readonly ArticleService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> Handle(ArticleCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = service.CreateArticle(
                command.Article.Name,
                command.Article.Description ?? default,
                command.Article.IsContainer);

            await unitOfWork.SaveChanges(cancellationToken);

            return article.Guid;
        }
    }
}