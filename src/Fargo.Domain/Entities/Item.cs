using Fargo.Domain.Collections;
using Fargo.Domain.Logics;

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

        public PartitionCollection Partitions
        {
            get;
            init;
        } = [];

        IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;
    }
}