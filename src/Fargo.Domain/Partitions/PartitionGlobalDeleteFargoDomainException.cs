namespace Fargo.Domain.Partitions;

/// <summary>
/// Exception thrown when an attempt is made to delete the global partition.
/// </summary>
public sealed class PartitionGlobalDeleteFargoDomainException()
    : FargoDomainException("The global partition cannot be deleted.")
{
}
