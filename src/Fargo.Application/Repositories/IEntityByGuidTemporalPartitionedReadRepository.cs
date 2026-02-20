using Fargo.Application.Commom;
using Fargo.Application.Models;

namespace Fargo.Application.Repositories
{
    public interface IEntityByGuidTemporalPartitionedReadRepository<TEntity>
        where TEntity : IEntityByGuidReadModel, IEntityTemporalReadModel
        {

        }
}