using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IArticleRepository
    {
        void Add(Article article);

        void Remove(Article article);

        Task<bool> HasItemsAssociated(
                Guid articleGuid,
                CancellationToken cancellationToken = default);

        Task<Article?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid> partitionGuids,
                CancellationToken cancellationToken = default
                );
    }
}