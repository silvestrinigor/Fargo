namespace Fargo.Core.Partitions;

/// <summary>
/// Exception thrown when an attempt is made to delete the global partition.
/// </summary>
public sealed class PartitionGlobalDeleteFargoCoreException()
    : FargoCoreException(
        "The global partition cannot be deleted.",
        FargoCoreErrorType.CannotDeleteGlobalPartition)
{
}
