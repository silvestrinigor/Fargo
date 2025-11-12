using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Services;

public class PartitionService(IPartitionRepository partitionRepository)
{
    private readonly IPartitionRepository partitionRepository = partitionRepository;

    public async Task InsertEntityIntoArea(Partition partition, Entity entity)
    {
        var fromPartition = await partitionRepository.GetEntityPartition(entity.Guid);
        fromPartition?.entities.Remove(entity);
        partition.entities.Add(entity);
    }
    public static void RemoveEntityFromPartition(Partition partition, Entity entity)
    {
        partition.entities.Remove(entity);
    }
}
