using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Repositories
{
    public interface IArticleReadRepository
    {
        Task<Article?> GetByGuidAsync(Guid articleGuid, DateTime? atDateTime = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<Article>> GetAllAsync(int? skip = null, int? take = null, CancellationToken cancellationToken = default);
    }
}
