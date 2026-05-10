using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.UserGroups;
using Fargo.Domain.Users;
using System.Collections.ObjectModel;

namespace Fargo.Domain.Partitions;

#region Entity

/// <summary>
/// Marker interface for domain entities that belong to one or more partitions.
/// </summary>
/// <remarks>
/// Implementing this interface signals that an entity participates in the
/// partition-based access control (PBAC) model, meaning its visibility and
/// mutability are governed by the actor's partition access set.
/// </remarks>
public interface IPartitionEntity : IEntity;

public interface IPartitioned
{
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}

/// <summary>
/// Represents an entity that is associated with one or more partitions.
/// </summary>
public interface IPartitionedEntity
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<IPartitionEntity> Partitions { get; }
}

/// <summary>
/// Represents an entity that has access to one or more partitions.
/// </summary>
public interface IPartitionUser
{
    /// <summary>
    /// Gets the collection of partition accesses granted to the user.
    /// </summary>
    IReadOnlyCollection<IPartitionAccess> PartitionAccesses { get; }
}

/// <summary>
/// Represents a user's access to a specific partition.
/// </summary>
public interface IPartitionAccess
{
    /// <summary>
    /// Gets the partition associated with this access.
    /// </summary>
    Partition Partition { get; }
}

/// <summary>
/// Represents a partition used to isolate and scope access to domain entities.
/// </summary>
/// <remarks>
/// Partitions define hierarchical access boundaries in the system.
///
/// A partition may reference a parent partition, forming a hierarchy.
/// Access inheritance flows from parent to child, but not from child to parent.
///
/// This means that a user with access to a parent partition can also access
/// entities belonging to its descendant partitions. However, a user with access
/// only to a child partition cannot access entities belonging to its parent
/// partition or to other branches of the hierarchy.
///
/// The system contains a unique global partition at the top of the hierarchy.
/// The global partition has access to all entities contained in its descendant
/// partitions. Access to this partition is restricted to highly privileged users.
/// </remarks>
public class Partition : ModifiedEntity, IPartitionEntity
{
    /// <summary>
    /// Gets or sets the name of the partition.
    /// </summary>
    /// <remarks>
    /// The name identifies the partition and must satisfy the validation
    /// rules defined by <see cref="Name"/>.
    /// </remarks>
    public required Name Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the partition.
    /// </summary>
    /// <remarks>
    /// This field provides additional contextual information about the
    /// purpose or scope of the partition. If not explicitly defined,
    /// it defaults to <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the partition is active.
    /// </summary>
    /// <remarks>
    /// Inactive partitions should not be considered available for new access
    /// assignments or operational use, depending on application rules.
    /// </remarks>
    public bool IsActive { get; set; } = true;

    #region ParentPartition

    /// <summary>
    /// Gets the unique identifier of the parent partition, if any.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the current partition
    /// is a root partition in the hierarchy.
    /// </remarks>
    public Guid? ParentPartitionGuid { get; private set; }

    /// <summary>
    /// Gets the parent partition of the current partition, if any.
    /// </summary>
    /// <remarks>
    /// The parent partition defines the hierarchical relationship between partitions,
    /// enabling access inheritance from parent to child.
    ///
    /// This property has an <see langword="internal"/> setter to ensure that changes
    /// to the partition hierarchy are controlled by the domain.
    ///
    /// Direct modification from outside the domain is restricted in order to:
    /// <list type="bullet">
    /// <item><description>Prevent circular hierarchies</description></item>
    /// <item><description>Enforce consistency of parent-child relationships</description></item>
    /// <item><description>Ensure domain invariants are validated before changes are applied</description></item>
    /// </list>
    ///
    /// Setting this property automatically updates <see cref="ParentPartitionGuid"/>
    /// to keep both properties consistent.
    /// </remarks>
    public Partition? ParentPartition
    {
        get;
        internal set
        {
            ParentPartitionGuid = value?.Guid;
            field = value;
        }
    }

    #endregion ParentPartition

    #region ChildPartition

    /// <summary>
    /// Gets the child partitions that belong to the current partition.
    /// </summary>
    /// <remarks>
    /// This collection represents the hierarchical relationship between partitions.
    /// Each member partition has the current partition as its parent.
    ///
    /// The collection is primarily used for navigation and persistence mapping.
    /// Domain logic should not rely on directly mutating this collection.
    /// </remarks>
    public IReadOnlyCollection<Partition> PartitionMembers
    {
        get => partitionMembers;
        init => partitionMembers = [.. value];
    }

    private readonly List<Partition> partitionMembers = [];

    #endregion ChildPartition

    #region Article

    /// <summary>
    /// Gets the articles associated with the current partition.
    /// </summary>
    /// <remarks>
    /// This collection represents the articles that belong to the partition.
    /// It is mainly used for persistence navigation and relationship mapping.
    /// Domain operations should interact with articles through their own
    /// aggregates and repositories.
    /// </remarks>
    public IReadOnlyCollection<Article> ArticleMembers
    {
        get => articleMembers;
        init => articleMembers = [.. value];
    }

    private readonly List<Article> articleMembers = [];

    #endregion Article

    #region Item

    /// <summary>
    /// Gets the items associated with the current partition.
    /// </summary>
    /// <remarks>
    /// This collection represents the items that belong to the partition.
    /// It is primarily intended for persistence navigation and relationship mapping.
    /// </remarks>
    public IReadOnlyCollection<Item> ItemMembers
    {
        get => itemMembers;
        init => itemMembers = [.. value];
    }

    private readonly List<Item> itemMembers = [];

    #endregion Item

    #region User

    /// <summary>
    /// Gets the users associated with the current partition.
    /// </summary>
    /// <remarks>
    /// This collection represents users that have membership or association
    /// with the partition. It is mainly used for persistence navigation.
    /// Authorization logic should rely on domain services or repositories
    /// rather than manipulating this collection directly.
    /// </remarks>
    public IReadOnlyCollection<User> UserMembers
    {
        get => userMembers;
        init => userMembers = [.. value];
    }

    private readonly List<User> userMembers = [];

    #endregion User

    #region UserGroup

    /// <summary>
    /// Gets the user groups associated with the current partition.
    /// </summary>
    /// <remarks>
    /// This collection represents user groups linked to the partition.
    /// It is primarily used for persistence navigation and relationship mapping.
    /// </remarks>
    public IReadOnlyCollection<UserGroup> UserGroupMembers
    {
        get => userGroupMembers;
        init => userGroupMembers = [.. value];
    }

    private readonly List<UserGroup> userGroupMembers = [];

    #endregion UserGroup
}

#endregion Entity

#region Collections

/// <summary>
/// Represents a collection of <see cref="Partition"/> instances associated with an entity.
/// </summary>
/// <remarks>
/// This collection enforces domain rules for entity partitions, such as preventing
/// duplicate items and rejecting <see langword="null"/> values.
/// </remarks>
public sealed class PartitionCollection : Collection<Partition>
{
    /// <summary>
    /// Initializes an empty <see cref="PartitionCollection"/>.
    /// </summary>
    public PartitionCollection()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="PartitionCollection"/> with the specified partitions.
    /// </summary>
    /// <param name="partitions">
    /// The partitions to populate the collection with.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partitions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection contains duplicate partitions.
    /// </exception>
    public PartitionCollection(IEnumerable<Partition> partitions)
    {
        ArgumentNullException.ThrowIfNull(partitions);

        foreach (var partition in partitions)
        {
            Add(partition);
        }
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, Partition item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item))
        {
            throw new InvalidOperationException(
                "The partition already exists in the collection.");
        }

        base.InsertItem(index, item);
    }

    /// <inheritdoc />
    protected override void SetItem(int index, Partition item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item) && !ReferenceEquals(Items[index], item))
        {
            throw new InvalidOperationException(
                "The partition already exists in the collection.");
        }

        base.SetItem(index, item);
    }
}

#endregion Collections

#region Repositories

/// <summary>
/// Defines the repository contract for managing <see cref="Partition"/> entities.
/// </summary>
public interface IPartitionRepository
{
    /// <summary>
    /// Retrieves a partition by its unique identifier.
    /// </summary>
    Task<Partition?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the unique identifiers of all descendant partitions of a given partition.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        Guid partitionGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the unique identifiers of all descendant partitions of the specified root partitions.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        IReadOnlyCollection<Guid> partitionGuids,
        bool includeRoots = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new partition to the persistence context.
    /// </summary>
    void Add(Partition partition);

    /// <summary>
    /// Removes a partition from the persistence context.
    /// </summary>
    void Remove(Partition partition);
}

#endregion Repositories

#region Services

/// <summary>
/// Provides domain operations related to partition retrieval
/// and partition-based access evaluation.
/// </summary>
/// <remarks>
/// This service encapsulates logic for retrieving partitions while enforcing
/// access rules based on the effective partition access of a <see cref="Fargo.Domain.Users.User"/>.
///
/// Effective access may be granted either:
/// <list type="bullet">
/// <item>
/// <description>directly to the user</description>
/// </item>
/// <item>
/// <description>indirectly through one of the user's <see cref="Fargo.Domain.UserGroups.UserGroup"/> memberships</description>
/// </item>
/// </list>
///
/// Access inheritance flows from parent to child. This means that a user
/// with access to a parent partition also has access to all of its descendant
/// partitions. Access does not flow from child to parent.
/// </remarks>
public class PartitionService(
    IPartitionRepository partitionRepository)
{
    /// <summary>
    /// The predefined unique identifier string representing
    /// the global partition.
    /// </summary>
    /// <remarks>
    /// The global partition is the root of the partition hierarchy
    /// and has implicit access to all descendant partitions.
    /// </remarks>
    private const string GlobalPartitionGuidString =
        "00000000-0000-0000-0000-000000000002";

    /// <summary>
    /// Gets the predefined unique identifier representing
    /// the global partition.
    /// </summary>
    /// <remarks>
    /// This GUID is reserved for the root partition of the system.
    /// It must remain constant across environments and is used
    /// to establish the top-level access scope for privileged users.
    /// </remarks>
    public static Guid GlobalPartitionGuid =>
        new(GlobalPartitionGuidString);

    /// <summary>
    /// Deletes the specified <see cref="Partition"/> from the system.
    /// </summary>
    /// <param name="partition">
    /// The partition to be deleted.
    /// </param>
    /// <exception cref="PartitionGlobalDeleteFargoDomainException">
    /// Thrown when an attempt is made to delete the global partition.
    /// </exception>
    /// <remarks>
    /// This operation removes the partition from the system.
    /// The global partition cannot be deleted under any circumstances.
    /// </remarks>
    public void DeletePartition(Partition partition)
    {
        if (partition.Guid == GlobalPartitionGuid)
        {
            throw new PartitionGlobalDeleteFargoDomainException();
        }

        partitionRepository.Remove(partition);
    }

    /// <summary>
    /// Sets the parent partition of a member partition.
    /// </summary>
    /// <param name="parentPartition">
    /// The partition that will become the parent.
    /// </param>
    /// <param name="memberPartition">
    /// The partition that will become a child of <paramref name="parentPartition"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentPartition"/> or
    /// <paramref name="memberPartition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="PartitionCannotBeOwnParentFargoDomainException">
    /// Thrown when a partition is assigned as its own parent.
    /// </exception>
    /// <exception cref="PartitionCircularHierarchyFargoDomainException">
    /// Thrown when assigning the parent would create a circular hierarchy.
    /// </exception>
    public async Task SetParentPartition(
        Partition parentPartition,
        Partition memberPartition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentPartition);
        ArgumentNullException.ThrowIfNull(memberPartition);

        if (parentPartition.Guid == memberPartition.Guid)
        {
            throw new PartitionCannotBeOwnParentFargoDomainException(
                memberPartition.Guid
            );
        }

        var createsCircularHierarchy =
            await CreatesCircularHierarchy(
                parentPartition,
                memberPartition.Guid,
                cancellationToken
            );

        if (createsCircularHierarchy)
        {
            throw new PartitionCircularHierarchyFargoDomainException(
                parentPartition.Guid,
                memberPartition.Guid
            );
        }

        memberPartition.ParentPartition = parentPartition;
    }

    private async Task<bool> CreatesCircularHierarchy(
        Partition candidateParentPartition,
        Guid memberPartitionGuid,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateParentPartition);

        if (candidateParentPartition.Guid == memberPartitionGuid)
        {
            return true;
        }

        var descendantPartitionGuids =
            await partitionRepository.GetDescendantGuids(
                memberPartitionGuid,
                false,
                cancellationToken
            );

        return descendantPartitionGuids.Contains(candidateParentPartition.Guid);
    }
}

#endregion Services

#region Exceptions

/// <summary>
/// Exception thrown when a partition is assigned as its own parent.
/// </summary>
public sealed class PartitionCannotBeOwnParentFargoDomainException(
    Guid partitionGuid
    ) : FargoDomainException(
        $"Partition '{partitionGuid}' cannot be its own parent.")
{
    /// <summary>
    /// Gets the identifier of the partition involved in the violation.
    /// </summary>
    public Guid PartitionGuid { get; } = partitionGuid;
}

/// <summary>
/// Exception thrown when an attempt is made to delete the global partition.
/// </summary>
public sealed class PartitionGlobalDeleteFargoDomainException()
    : FargoDomainException("The global partition cannot be deleted.")
{
}

/// <summary>
/// Exception thrown when a partition hierarchy would become circular.
/// </summary>
public sealed class PartitionCircularHierarchyFargoDomainException(
    Guid parentPartitionGuid,
    Guid memberPartitionGuid
    ) : FargoDomainException(
        $"Partition '{memberPartitionGuid}' cannot be assigned to parent " +
        $"'{parentPartitionGuid}' because this would create a circular hierarchy.")
{
    /// <summary>
    /// Gets the identifier of the candidate parent partition.
    /// </summary>
    public Guid ParentPartitionGuid { get; } = parentPartitionGuid;

    /// <summary>
    /// Gets the identifier of the member partition.
    /// </summary>
    public Guid MemberPartitionGuid { get; } = memberPartitionGuid;
}

#endregion Exceptions
