using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Models.ArticleModels
{
    public record class ArticleReadModel(
            Guid Guid,
            string Name,
            string Description,
            bool IsContainer,
            IReadOnlyCollection<PartitionReadModel> Partitions
            ) : IEntityByGuidReadModel, IEntityTemporalReadModel;
}