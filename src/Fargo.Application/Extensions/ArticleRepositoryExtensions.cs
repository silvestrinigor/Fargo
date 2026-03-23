using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

public static class ArticleRepositoryExtensions
{
    extension(IArticleRepository repository)
    {
        public async Task<Article> GetFoundByGuid(Guid articleGuid, CancellationToken cancellationToken = default)
        {
            var article = await repository.GetByGuid(articleGuid, cancellationToken)
                ?? throw new ArticleNotFoundFargoApplicationException(articleGuid);

            return article;
        }
    }
}
