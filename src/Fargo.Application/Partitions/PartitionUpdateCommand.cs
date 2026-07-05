using Fargo.Application.Shared.Partitions;

namespace Fargo.Application.Partitions;

/// <summary>
/// Command used to update an existing partition from an API update payload.
/// </summary>
public sealed record PartitionUpdateCommand(
    Guid PartitionGuid,
    PartitionUpdateDto Update
) : ICommand;