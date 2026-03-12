namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents an item in the system.
    ///
    /// An item is an individual instance associated with a specific
    /// <see cref="Article"/>. While an <see cref="Article"/> defines the
    /// descriptive information of a product, an <see cref="Item"/> represents
    /// a concrete unit of that article.
    /// </summary>
    /// <remarks>
    /// An item is partitioned data and defines its own partition scope
    /// independently of the associated <see cref="Article"/>.
    ///
    /// Although the item is related to an article, access to the item is not
    /// determined by the article's partitions. Instead, a user may access the item
    /// only if they have access to at least one partition associated directly
    /// with the item, subject to additional authorization rules.
    /// </remarks>
    public class Item : AuditedEntity, IPartitioned
    {
        /// <summary>
        /// Gets the unique identifier of the associated <see cref="Article"/>.
        /// </summary>
        public Guid ArticleGuid
        {
            get;
            private init;
        }

        /// <summary>
        /// Gets the article associated with this item.
        /// </summary>
        /// <remarks>
        /// The associated article defines the descriptive classification of the item,
        /// but does not determine the partition access scope of this entity.
        ///
        /// When the article is assigned, <see cref="ArticleGuid"/>
        /// is automatically synchronized with the article's identifier.
        /// </remarks>
        public required Article Article
        {
            get;
            init
            {
                ArticleGuid = value.Guid;
                field = value;
            }
        }

        /// <summary>
        /// Gets the read-only collection of partitions to which the item belongs.
        /// </summary>
        /// <remarks>
        /// These partitions define the access scope of the item itself.
        /// A user may access the item only if they have access to at least one
        /// of these partitions.
        /// </remarks>
        public IReadOnlyCollection<Partition> Partitions
        {
            get => partitions;
            init => partitions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store the partitions
        /// associated with the item.
        /// </summary>
        private readonly List<Partition> partitions = [];

        /// <summary>
        /// Adds the specified partition to the item if it is not already associated.
        /// </summary>
        /// <param name="partition">The partition to associate with the item.</param>
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
        /// Removes the specified partition from the item if it exists.
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