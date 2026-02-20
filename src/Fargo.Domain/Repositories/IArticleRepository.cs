using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IArticleRepository
    {
        Task<Article?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        Task<bool> HasItemsAssociated(
                Guid articleGuid,
                CancellationToken cancellationToken = default
                );

        void Add(Article article);

        void Remove(Article article);
    }
}