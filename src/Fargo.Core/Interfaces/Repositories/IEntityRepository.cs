using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Interfaces.Repositories;

public interface IEntityRepository<TEntity> where TEntity : NamedEntity
{
    Task<TEntity?> GetAsync(Guid guid);
    Task<IEnumerable<TEntity>> GetAsync();
    Task<IEnumerable<Guid>> GetGuidsAsync();
    void Add(TEntity entity);
    void Remove(TEntity entity);
}
