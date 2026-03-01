using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.PartitionModels
{
    public sealed record PartitionResponseModel(
            Guid Guid,
            Name Name,
            Description Description
            );
}