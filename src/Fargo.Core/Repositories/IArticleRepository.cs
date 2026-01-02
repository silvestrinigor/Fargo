using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IArticleRepository
    {
        Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);
        void Add(Article article);
        void Remove(Article article);
        Task<bool> HasItensAssociated(Guid articleGuid, CancellationToken cancellationToken = default);
    }
}