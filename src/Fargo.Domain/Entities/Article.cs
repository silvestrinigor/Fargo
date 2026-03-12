using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents an article in the system.
    ///
    /// An article defines the descriptive information of a product or item type,
    /// such as its name and description. It does not represent a physical unit,
    /// but rather the metadata describing a type of item.
    /// </summary>
    /// <remarks>
    /// An article is partitioned data and may belong to multiple <see cref="Partition"/> instances.
    /// Users can only access the article if they have access to at least one of its partitions.
    /// </remarks>
    public class Article : AuditedEntity, IPartitioned
    {
        /// <summary>
        /// Gets or sets the name of the article.
        /// </summary>
        /// <remarks>
        /// The name uniquely identifies the article in the domain context
        /// and must satisfy the validation rules defined in <see cref="Name"/>.
        /// </remarks>
        public required Name Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the article.
        /// </summary>
        /// <remarks>
        /// If no description is provided, the default value is
        /// <see cref="Description.Empty"/>.
        /// </remarks>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets the read-only collection of partitions to which the article belongs.
        /// </summary>
        /// <remarks>
        /// These partitions define the data access scope of the article.
        /// A user can access the article only if they have access to at least
        /// one of these partitions.
        /// </remarks>
        public IReadOnlyCollection<Partition> Partitions
        {
            get => partitions;
            init => partitions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store the partitions
        /// associated with the article.
        /// </summary>
        private readonly List<Partition> partitions = [];

        /// <summary>
        /// Adds the specified partition to the article if it is not already associated.
        /// </summary>
        /// <param name="partition">The partition to associate with the article.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
        /// </exception>
        public void AddPartition(Partition partition)
        {
            ArgumentNullException.ThrowIfNull(partition);

            if (partitions.Any(x => x.Guid == partition.Guid))
            {
                return;
            }

            partitions.Add(partition);
        }

        /// <summary>
        /// Removes the specified partition from the article if it exists.
        /// </summary>
        /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
        public void RemovePartition(Guid partitionGuid)
        {
            var partition = partitions.SingleOrDefault(x => x.Guid == partitionGuid);

            if (partition == null)
            {
                return;
            }

            partitions.Remove(partition);
        }
    }
}