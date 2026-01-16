using Fargo.Application.Commom;
using Fargo.Application.Models;

namespace Fargo.Application.Repositories
{
    public interface IEntityByGuidTemporalReadRepository<TEntity>
        where TEntity : IEntityByGuidReadModel, IEntityTemporalReadModel
    {
        Task<TEntity?> GetByGuidAsync(
            Guid entityGuid,
            DateTime? asOfDateTime = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetManyAsync(
            DateTime? asOfDateTime = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default);
    }
}