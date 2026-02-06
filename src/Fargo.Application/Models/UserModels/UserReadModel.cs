using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserReadModel(
            Guid Guid,
            string Name,
            string Description,
            IReadOnlyCollection<PartitionReadModel> Partitions
            ) : IEntityByGuidReadModel, IEntityTemporalReadModel, IEntityPartitionedReadModel;
}