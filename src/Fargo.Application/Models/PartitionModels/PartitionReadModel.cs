using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.PartitionModels
{
    /// <summary>
    /// Represents the data returned when reading a <c>Partition</c>.
    /// </summary>
    /// <remarks>
    /// This model exposes the partition state for read operations,
    /// including its identity, descriptive information, activation status,
    /// and the identifier of its parent partition when the partition
    /// belongs to a hierarchy.
    /// </remarks>
    public class PartitionReadModel : AuditedEntityReadModel
    {
        /// <summary>
        /// Gets the name of the partition.
        /// </summary>
        /// <remarks>
        /// The name uniquely identifies the partition within the domain
        /// according to the rules defined by <see cref="Name"/>.
        /// </remarks>
        public required Name Name
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the description of the partition.
        /// </summary>
        /// <remarks>
        /// This value provides additional contextual information about the
        /// purpose or scope of the partition.
        /// </remarks>
        public required Description Description
        {
            get;
            init;
        }

        /// <summary>
        /// Gets a value indicating whether the partition is active.
        /// </summary>
        /// <remarks>
        /// An active partition is available for normal use according to
        /// the application rules.
        /// </remarks>
        public required bool IsActive
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the unique identifier of the parent partition, if any.
        /// </summary>
        /// <remarks>
        /// A <see langword="null"/> value indicates that the partition
        /// does not have a parent and is therefore a root partition
        /// in the hierarchy.
        /// </remarks>
        public required Guid? ParentPartitionGuid
        {
            get;
            init;
        }
    }
}