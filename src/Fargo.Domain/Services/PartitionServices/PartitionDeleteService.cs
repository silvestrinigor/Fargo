using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.PartitionServices
{
    public class PartitionDeleteService(
            IPartitionRepository partitionRepository
            )
    {
        public void DeletePartition(
                User actor,
                Partition partition
                )
        {
            actor.ValidatePermission(ActionType.DeletePartition);

            partitionRepository.Remove(partition);
        }
    }
}