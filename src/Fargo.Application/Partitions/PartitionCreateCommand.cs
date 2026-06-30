using Fargo.Application.Shared.Partitions;

namespace Fargo.Application.Partitions;

/// <summary>
/// Command used to create a new partition from an API creation payload.
/// </summary>
public sealed record PartitionCreateCommand(
    PartitionCreateDto Create
) : ICommand<Guid>;