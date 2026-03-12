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
    public class Partition : AuditedEntity
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
        /// Gets or sets a value indicating whether this partition is the unique
        /// global partition of the system.
        /// </summary>
        /// <remarks>
        /// The global partition is the top-level partition in the hierarchy and
        /// has implicit access to all entities contained in its descendant partitions.
        /// This partition is intended for highly privileged users only.
        /// </remarks>
        public bool IsGlobal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this partition can be managed.
        /// </summary>
        /// <remarks>
        /// The global partition should typically not be editable by regular users.
        /// This property can be used to protect partitions from normal management
        /// operations when required by business rules.
        /// </remarks>
        public bool IsEditable
        {
            get;
            set;
        } = true;

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