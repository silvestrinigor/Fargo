using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IArticleRepository : IEntityByGuidRepository<Article>
    {
        void Add(Article article);

        void Remove(Article article);

        Task<bool> HasItemsAssociated(
            Guid articleGuid,
            CancellationToken cancellationToken = default);
    }
}