using Fargo.Core.Partitions;

namespace Fargo.Core.ServiceAccounts;

/// <summary>
/// Represents the association between a service account and a partition in
/// which the service account belongs.
/// </summary>
/// <remarks>
/// This association identifies the partitions to which the service account is
/// assigned as a member. Membership defines the account's partition scope and
/// is distinct from partition access (which governs the partitions the account
/// is allowed to reach).
/// </remarks>
public class ServiceAccountPartition
{
    /// <summary>
    /// Gets the unique identifier of the associated service account.
    /// </summary>
    public Guid ServiceAccountGuid { get; private init; }

    /// <summary>
    /// Gets the associated service account.
    /// </summary>
    public ServiceAccount ServiceAccount { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the partition containing the service account.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the partition in which the service account belongs.
    /// </summary>
    public Partition Partition { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAccountPartition"/> class.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ServiceAccountPartition()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    /// <summary>
    /// Initializes a new service-account-partition membership.
    /// </summary>
    /// <param name="serviceAccount">The service account assigned to the partition.</param>
    /// <param name="partition">The partition to which the service account belongs.</param>
    internal ServiceAccountPartition(ServiceAccount serviceAccount, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(serviceAccount);
        ArgumentNullException.ThrowIfNull(partition);

        ServiceAccount = serviceAccount;
        ServiceAccountGuid = serviceAccount.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new service-account-partition membership using the
    /// specified partition identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not
    /// need to be loaded, such as when assigning the account to a well-known
    /// or global partition whose identifier is already known and stable.
    ///
    /// Prefer this constructor over the constructor that accepts a
    /// <see cref="Partition"/> when only the partition identifier is required.
    /// This avoids requiring the partition entity to be loaded solely for
    /// creating the association.
    /// </remarks>
    /// <param name="serviceAccount">
    /// The service account assigned to the partition.
    /// </param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition to which the service account belongs.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceAccount"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is
    /// <see cref="Guid.Empty"/>.
    /// </exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal ServiceAccountPartition(ServiceAccount serviceAccount, Guid partitionGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(serviceAccount);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        ServiceAccount = serviceAccount;
        ServiceAccountGuid = serviceAccount.Guid;

        PartitionGuid = partitionGuid;
    }
}
