using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Repositories
{
    public interface IPartitionReadRepository : IEntityByGuidTemporalReadRepository<PartitionReadModel>;
}
