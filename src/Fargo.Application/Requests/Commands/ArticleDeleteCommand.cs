using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands
{
    public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand;

    public sealed class ArticleDeleteCommandHandler(IArticleRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ArticleDeleteCommand>
    {
        public async Task HandleAsync(ArticleDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var hasItens = await repository.HasItensAssociated(command.ArticleGuid, cancellationToken);

            if (hasItens)
            {
                throw new InvalidOperationException("Cannot delete article with associated items.");
            }

            var article = await repository.GetByGuidAsync(command.ArticleGuid, cancellationToken)
                ?? throw new InvalidOperationException("Article not found.");

            repository.Remove(article);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
