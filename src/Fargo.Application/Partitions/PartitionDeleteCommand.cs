namespace Fargo.Application.Partitions;

/// <summary>
/// Command used to delete a partition.
/// </summary>
public sealed record PartitionDeleteCommand(
    Guid PartitionGuid
) : ICommand;
