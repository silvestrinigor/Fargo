using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
        ArticleCreateDto Article
        ) : ICommand<Guid>;

    public sealed class ArticleCreateCommandHandler(IArticleRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ArticleCreateCommand, Guid>
    {
        public async Task<Guid> HandleAsync(ArticleCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = new Article
            {
                Name = command.Article.Name,
                Description = command.Article.Description,
            };
            
            repository.Add(article);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return article.Guid;
        }
    }
}