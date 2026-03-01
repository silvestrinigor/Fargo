using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserReadModel(
            Guid Guid,
            Nameid Nameid,
            Description Description,
            IReadOnlyCollection<PartitionReadModel> Partitions,
            IReadOnlyCollection<ActionType> Permissions
            );
}