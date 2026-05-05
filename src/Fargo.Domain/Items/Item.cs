using Fargo.Domain.Articles;
using Fargo.Domain.Partitions;

namespace Fargo.Domain.Items;

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
/// if the item has no partition (public), or if they have access to at least
/// one partition associated directly with the item.
/// </remarks>
public class Item : ModifiedEntity, IPartitionedEntity
{
    public Item(Article article)
    {
        if (article.IsContainer)
        {
            Container = new ItemContainer(this);
        }

        Article = article;
    }

    #region Article

    /// <summary>
    /// Gets the unique identifier of the associated <see cref="Article"/>.
    /// </summary>
    /// <remarks>
    /// This value is synchronized with <see cref="Article"/> when the item is initialized.
    /// </remarks>
    public Guid ArticleGuid { get; private init; }

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
    public Article Article
    {
        get;
        private init
        {
            ArticleGuid = value.Guid;
            field = value;
        }
    }

    #endregion Article

    /// <summary>
    /// Gets the date on which this item was produced.
    /// When <see langword="null"/>, the production date is unknown.
    /// </summary>
    public DateTimeOffset? ProductionDate { get; init; }

    /// <summary>
    /// Gets the date the item will expire.
    /// When <see langword="null"/>, the expiration date is unknown.
    /// </summary>
    public DateTimeOffset? ExpirationDate => ProductionDate + Article.ShelfLife;

    #region Container

    /// <summary>
    /// Gets the parent container of the current item, if any.
    /// </summary>
    public ItemContainer? ParentContainer
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets the container information of the current item, if the current item is a container.
    /// </summary>
    public ItemContainer? Container
    {
        get;
        private init;
    }

    public bool? IsContainer => Article.Container is not null;

    #endregion Container

    #region  Partition

    /// <summary>
    /// Gets the partitions associated with the item.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the item and are used
    /// in partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;

    #endregion  Partition
}

public sealed class ItemContainer
{
    public ItemContainer(Item item)
    {
        Item = item;
    }

    public Item Item { get; private init; }

}