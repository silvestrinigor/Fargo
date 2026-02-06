using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Models
{
    public interface IEntityPartitionedReadModel
    {
        public IReadOnlyCollection<PartitionReadModel> Partitions
        {
            get;
        }
    }
}