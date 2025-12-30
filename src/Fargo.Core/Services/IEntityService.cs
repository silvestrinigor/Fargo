using Fargo.Domain.Entities;

namespace Fargo.Domain.Services
{
    public interface IEntityService<TEntity> where TEntity : Entity
    {
        TEntity CreateNewEntity(TEntity entity);
    }
}
