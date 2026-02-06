using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Models.ItemModels
{
    public record ItemReadModel(
            Guid Guid,
            Guid ArticleGuid,
            Guid? ParentItemGuid,
            IReadOnlyCollection<PartitionReadModel> Partitions
            ) : IEntityByGuidReadModel, IEntityTemporalReadModel, IEntityPartitionedReadModel;
}