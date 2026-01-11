using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories
{
    public interface IEntityByGuidTemporalReadRepository<TEntity> where TEntity : class, IEntityByGuid, IEntityTemporal
    {
        Task<TEntity?> GetByGuidAsync(
            Guid entityGuid,
            DateTime? atDateTime = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetManyAsync(
            DateTime? atDateTime = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default);
    }
}