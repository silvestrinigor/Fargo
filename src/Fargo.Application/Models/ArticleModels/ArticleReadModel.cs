using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    public record class ArticleReadModel(
            Guid Guid,
            Name Name,
            Description Description,
            bool IsContainer,
            IReadOnlyCollection<PartitionReadModel> Partitions
            );
}