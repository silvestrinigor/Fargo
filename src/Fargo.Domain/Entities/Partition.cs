using Fargo.Domain.Logics;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
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
    public class Partition : AuditedEntity, IPartition
    {
        /// <summary>
        /// Gets or sets the name of the partition.
        /// </summary>
        /// <remarks>
        /// The name identifies the partition and must satisfy the validation
        /// rules defined by <see cref="Name"/>.
        /// </remarks>
        public required Name Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the partition.
        /// </summary>
        /// <remarks>
        /// This field provides additional contextual information about the
        /// purpose or scope of the partition. If not explicitly defined,
        /// it defaults to <see cref="Description.Empty"/>.
        /// </remarks>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the partition is active.
        /// </summary>
        /// <remarks>
        /// Inactive partitions should not be considered available for new access
        /// assignments or operational use, depending on application rules.
        /// </remarks>
        public bool IsActive
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets the unique identifier of the parent partition, if any.
        /// </summary>
        /// <remarks>
        /// A <see langword="null"/> value indicates that the current partition
        /// is a root partition in the hierarchy.
        /// </remarks>
        public Guid? ParentPartitionGuid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parent partition of the current partition, if any.
        /// </summary>
        /// <remarks>
        /// The parent partition defines the hierarchical relationship used
        /// for access inheritance from parent to child.
        /// </remarks>
        public Partition? ParentPartition
        {
            get;
            private set;
        }

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

        /// <summary>
        /// Marks the partition as active.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Marks the partition as inactive.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Sets the parent partition of the current partition.
        /// </summary>
        /// <param name="parentPartition">The parent partition to associate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parentPartition"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Setting a parent partition places the current partition under the specified
        /// partition in the hierarchy, causing access inheritance to flow from the
        /// parent to the current partition.
        /// </remarks>
        public void SetParentPartition(Partition parentPartition)
        {
            ArgumentNullException.ThrowIfNull(parentPartition);

            ParentPartition = parentPartition;
            ParentPartitionGuid = parentPartition.Guid;
        }

        /// <summary>
        /// Removes the parent partition association from the current partition.
        /// </summary>
        /// <remarks>
        /// After calling this method, the current partition becomes a root partition
        /// in the hierarchy.
        /// </remarks>
        public void ClearParentPartition()
        {
            ParentPartition = null;
            ParentPartitionGuid = null;
        }
    }
}