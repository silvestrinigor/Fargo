using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public class PartitionService(IPartitionRepository repository)
    {
        private readonly IPartitionRepository repository = repository;

        public async Task<Partition> GetPartitionAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(partitionGuid, cancellationToken)
            ?? throw new InvalidOperationException("Partition not found.");

        public Partition CreatePartition(Name name, Description description = default)
        {
            var partition = new Partition
            {
                Name = name,
                Description = description
            };

            repository.Add(partition);

            return partition;
        }

        public void DeletePartition(Partition partition)
            => repository.Remove(partition);
    }
}
