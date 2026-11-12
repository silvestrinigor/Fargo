using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.Items;

/// <summary>
/// An individual physical unit of an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// An item defines its own partition scope independently of its article: a user may access it if
/// it has no partition (public) or if they have access to at least one of its own partitions.
/// Every item belongs to the global partition, which cannot be removed.
/// </remarks>
public class Item : Entity, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    /// <summary>
    /// <see langword="null"/> when the item is not placed inside another item.
    /// </summary>
    public Guid? ParentItemContainerGuid { get; private set; }

    /// <summary>
    /// <see langword="null"/> when the item is not placed inside another item.
    /// </summary>
    public Item? ParentItemContainer { get; private set; }

    /// <summary>
    /// A fixed item can be neither moved into another container nor removed from its container.
    /// </summary>
    public bool IsFixed { get; private set; } = false;

    public IReadOnlyCollection<ItemPartition> Partitions => partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ItemPartition> partitions = [];

    /// <summary>
    /// A movement is recorded whenever the parent container changes; removing the item from a
    /// container records a movement with no destination.
    /// </summary>
    public IReadOnlyCollection<ItemMoviment> Moviments => moviments;

    private readonly List<ItemMoviment> moviments = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Required by EF Core.
    private Item()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        EntityType = EntityType.Item;
    }

    private Item(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        EntityType = EntityType.Item;

        if (article.TrackingMode != ArticleTrackingMode.Entity)
        {
            throw new FargoCoreException(
                $"An item cannot be created for article '{article.Guid}' because it is not entity-tracked. Event-tracked articles record stock through movement events instead of individual items.",
                FargoErrorType.InvalidOperation);
        }

        Article = article;
        ArticleGuid = article.Guid;

        partitions.Add(new ItemPartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    /// <exception cref="FargoCoreException">
    /// The article is not entity-tracked.
    /// </exception>
    public static Item CreateItem(Article article)
        => new(article);

    /// <summary>
    /// Places the item inside a container item and records a movement.
    /// </summary>
    /// <remarks>
    /// Checks only what the item and the parent show: the parent is not the item itself, is a
    /// container article, and the item is not fixed. Checks that traverse the hierarchy, such as
    /// indirect circular references, are done by <see cref="ItemService"/>.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// The parent is the item itself or not a container, or the item is fixed.
    /// </exception>
    public void PlaceInsideContainer(Item parentItemContainer)
    {
        ArgumentNullException.ThrowIfNull(parentItemContainer);

        if (parentItemContainer.Guid == Guid)
        {
            throw new FargoCoreException($"Item '{Guid}' cannot be its own parent container.", FargoErrorType.InvalidOperation);
        }

        if (parentItemContainer.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException(
                $"Item '{parentItemContainer.Guid}' is not a container item.", FargoErrorType.InvalidOperation);
        }

        if (IsFixed)
        {
            throw new FargoCoreException(
                $"The fixed item {Guid} cannot be moved to container {parentItemContainer.Guid}.",
                FargoErrorType.InvalidOperation);
        }

        ParentItemContainer = parentItemContainer;

        ParentItemContainerGuid = parentItemContainer.Guid;

        moviments.Add(new ItemMoviment(Guid, parentItemContainer.Guid, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Clears the parent container and records a movement with no destination.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The item is fixed.
    /// </exception>
    public void RemoveFromContainers()
    {
        if (IsFixed)
        {
            throw new FargoCoreException(
                $"The fixed item {Guid} cannot be removed from container.",
                FargoErrorType.InvalidOperation);
        }

        ParentItemContainer = null;

        ParentItemContainerGuid = null;

        moviments.Add(new ItemMoviment(Guid, null, DateTimeOffset.UtcNow));
    }

    /// <remarks>
    /// Idempotent; the parent container is unchanged.
    /// </remarks>
    public void Fix()
    {
        IsFixed = true;
    }

    /// <remarks>
    /// Idempotent; the parent container is unchanged.
    /// </remarks>
    public void Unfix()
    {
        IsFixed = false;
    }

    /// <remarks>
    /// Idempotent: nothing happens if the item already has the partition.
    /// </remarks>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new ItemPartition(this, partition));
    }

    /// <remarks>
    /// Nothing happens if the item does not have the partition.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// <paramref name="partitionGuid"/> is the global partition, which is mandatory.
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                "The global partition is mandatory and cannot be removed from an item.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }
}
