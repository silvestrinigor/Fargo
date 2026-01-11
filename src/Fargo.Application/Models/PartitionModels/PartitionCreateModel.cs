using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.PartitionModels
{
    public sealed record PartitionCreateModel(
        Name Name,
        Description? Description = null);
}
