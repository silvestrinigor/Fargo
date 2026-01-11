using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.PartitionModels
{
    public sealed record PartitionUpdateModel(
        Name? Name,
        Description? Description);
}
