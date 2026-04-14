using Fargo.Domain.Collections;

namespace Fargo.Domain.Entities;

// TODO: validate documentation
/// <summary>
/// Represents an item in the system.
/// </summary>
/// <remarks>
/// An item is a concrete instance associated with a specific
/// <see cref="Article"/>. While an <see cref="Article"/> defines the
/// descriptive information of a product type, an <see cref="Item"/>
/// represents an individual unit of that article.
///
/// An item is partitioned data and defines its own partition scope
/// independently of the associated <see cref="Article"/>.
///
/// Although the item is related to an article, access to the item is not
/// determined by the article's partitions. Instead, a user may access the item
/// only if they have access to at least one partition associated directly
/// with the item, subject to additional authorization rules.
/// </remarks>
public class Item : ModifiedEntity, IPartitionedEntity
{
    /// <summary>
    /// Gets the unique identifier of the associated <see cref="Article"/>.
    /// </summary>
    /// <remarks>
    /// This value is synchronized with <see cref="Article"/> when the item is initialized.
    /// </remarks>
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
    /// When this property is initialized, <see cref="ArticleGuid"/> is
    /// automatically set to the identifier of the assigned article.
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
    /// Gets the partitions associated with the item.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the item and are used
    /// in partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions
    {
        get;
        init;
    } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;
}
