using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IEntityByGuidRepository<TEntity>
        where TEntity : class, IEntityByGuid, IEntityPartitioned
    {
        Task<TEntity?> GetByGuid(
            Guid entityGuid,
            IReadOnlyCollection<Guid> partitionGuids,
            CancellationToken cancellationToken = default
            );
    }
}