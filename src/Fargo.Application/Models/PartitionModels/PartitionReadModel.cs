using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.PartitionModels
{
    public sealed record PartitionReadModel(
            Guid Guid,
            Name Name,
            Description Description,
            IReadOnlyCollection<PartitionReadModel> Partitions
            );
}