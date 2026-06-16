using Fargo.Core.Activables;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Modifiables;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using System.Collections.ObjectModel;

namespace Fargo.Core.Partitions;

#region Entity

/// <summary>
/// Marker interface for domain entities that belong to one or more partitions.
/// </summary>
/// <remarks>
/// Implementing this interface signals that an entity participates in the
/// partition-based access control (PBAC) model, meaning its visibility and
/// mutability are governed by the actor's partition access set.
/// </remarks>
public interface IPartition : IEntity;

public interface IPartitionedGuids : IEntity
{
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}

/// <summary>
/// Represents an entity that is associated with one or more partitions.
/// </summary>
public interface IPartitioned : IEntity
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<IPartition> Partitions { get; }

    void AddPartition(Partition partition);

    void RemovePartition(Partition partition);
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
public class Partition : Entity, IModifiable, IModifiableTypes<PartitionModifiedType>, IPartition, IActivable
{
    public static Partition CreatePartition(Name name, Description? description = null)
        => new(name, description);

    public static Partition CreatePartition(Guid guid, Name name, Description? description = null)
        => new(name, description)
        {
            Guid = guid
        };

    public static Partition CreatePartition(Name name, Actor actor, Description? description = null)
    {
        ArgumentNullException.ThrowIfNull(actor);
        actor.ThrowIfPermissionNotAuthorized(ActionType.CreatePartition);

        var partition = new Partition(name, description);

        partition.MarkAsEditedBy(actor.Guid);
        partition.MarkModificationType(PartitionModifiedType.General);

        return partition;
    }

    public static Partition CreatePartition(Guid guid, Name name, Actor actor, Description? description = null)
    {
        ArgumentNullException.ThrowIfNull(actor);
        actor.ThrowIfPermissionNotAuthorized(ActionType.CreatePartition);

        var partition = new Partition(name, description)
        {
            Guid = guid
        };

        partition.MarkAsEditedBy(actor.Guid);
        partition.MarkModificationType(PartitionModifiedType.General);

        return partition;
    }

    private Partition()
    {
    }

    private Partition(Name name, Description? description = null)
    {
        Name = name;
        Description = description ?? Description.Empty;
    }

    /// <summary>
    /// Gets or sets the name of the partition.
    /// </summary>
    /// <remarks>
    /// The name identifies the partition and must satisfy the validation
    /// rules defined by <see cref="Name"/>.
    /// </remarks>
    public Name Name { get; private set; }

    /// <summary>
    /// Gets or sets the description of the partition.
    /// </summary>
    /// <remarks>
    /// This field provides additional contextual information about the
    /// purpose or scope of the partition. If not explicitly defined,
    /// it defaults to <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description { get; private set; } = Description.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the partition is active.
    /// </summary>
    /// <remarks>
    /// Inactive partitions should not be considered available for new access
    /// assignments or operational use, depending on application rules.
    /// </remarks>
    public bool IsActive { get; private set; } = true;

    public void Rename(Name name)
    {
        if (Name == name)
        {
            return;
        }

        Name = name;
    }

    public void Rename(Name name, Actor actor)
    {
        ValidateCanEdit(actor);

        if (Name == name)
        {
            return;
        }

        Name = name;

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(PartitionModifiedType.General);
    }

    public void ChangeDescription(Description description)
    {
        if (Description == description)
        {
            return;
        }

        Description = description;
    }

    public void ChangeDescription(Description description, Actor actor)
    {
        ValidateCanEdit(actor);

        if (Description == description)
        {
            return;
        }

        Description = description;

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(PartitionModifiedType.General);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void Activate(Actor actor)
    {
        ValidateCanEdit(actor);

        if (IsActive)
        {
            return;
        }

        IsActive = true;

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(PartitionModifiedType.Activated);
    }

    public void Deactivate(Actor actor)
    {
        ValidateCanEdit(actor);

        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(PartitionModifiedType.Deactivated);
    }

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

    #region Modified

    public void ValidateCanEdit(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.EditPartition);
        actor.ValidateHasAccess(Guid);
    }

    public void ValidateCanDelete(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.DeletePartition);
        actor.ValidateHasAccess(Guid);
    }

    public Guid? EditedByActorid { get; private set; }

    public void MarkAsEditedBy(Guid actorGuid)
    {
        EditedByActorid = actorGuid;
    }

    public PartitionModifiedType ModificationTypes { get; private set; }

    public IReadOnlySet<PartitionModifiedType> GetModificationTypes()
    {
        HashSet<PartitionModifiedType> result = [];

        foreach (PartitionModifiedType value in Enum.GetValues<PartitionModifiedType>())
        {
            if (value == PartitionModifiedType.None)
            {
                continue;
            }

            if ((ModificationTypes & value) != 0)
            {
                result.Add(value);
            }
        }

        return result;
    }

    public bool IsEditStarted { get; private set; }

    public void MarkModificationType(PartitionModifiedType modificationType)
    {
        if (!IsEditStarted)
        {
            ModificationTypes = PartitionModifiedType.None;
            IsEditStarted = true;
        }

        ModificationTypes |= modificationType;
    }

    #endregion Modified
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
