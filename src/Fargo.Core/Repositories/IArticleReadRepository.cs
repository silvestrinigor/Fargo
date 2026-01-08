using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Repositories
{
    public interface IArticleReadRepository
    {
        Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);

        Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
