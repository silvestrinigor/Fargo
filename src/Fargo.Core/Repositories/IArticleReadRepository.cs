using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IArticleReadRepository
    {
        Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);

        Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<bool> HasItensAssociated(Guid articleGuid, CancellationToken cancellationToken = default);
    }
}
