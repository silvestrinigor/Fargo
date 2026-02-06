using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public class PartitionReadRepository(FargoReadDbContext context) : EntityByGuidTemporalPartitionedReadRepository<PartitionReadModel>(context.Partitions), IPartitionReadRepository;
}