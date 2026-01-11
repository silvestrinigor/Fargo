using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IEntityByGuidRepository<TEntity> where TEntity : class, IEntityByGuid
    {
        Task<TEntity?> GetByGuidAsync(
            Guid entityGuid,
            CancellationToken cancellationToken = default);
    }
}
