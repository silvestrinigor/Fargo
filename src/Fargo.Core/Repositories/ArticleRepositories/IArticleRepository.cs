using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories.ArticleRepositories
{
    public interface IArticleRepository : IEntityByGuidRepository<Article>
    {
        void Add(Article article);

        void Remove(Article article);

        Task<bool> HasItensAssociated(
            Guid articleGuid,
            CancellationToken cancellationToken = default);
    }
}