using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services.PartitionServices
{
    public class PartitionCreateService(
            IPartitionRepository partitionRepository
            )
    {
        public Partition CreatePartition(
                User actor,
                Name name
                )
        {
            actor.ValidatePermission(ActionType.CreatePartition);

            var partition = new Partition
            {
                Name = name,
                UpdatedBy = actor
            };

            partitionRepository.Add(partition);

            return partition;
        }
    }
}