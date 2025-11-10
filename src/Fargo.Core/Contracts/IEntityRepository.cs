using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Contracts;

public interface IEntityRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetAsync(Guid guid);
    Task<IEnumerable<TEntity>> GetAsync();
    Task<IEnumerable<Guid>> GetGuidsAsync();
    void Add(TEntity entity);
    void Remove(TEntity entity);
}
