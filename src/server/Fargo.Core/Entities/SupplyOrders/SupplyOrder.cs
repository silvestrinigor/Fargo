using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using UnitsNet;

namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// A document representing stock that is expected to be delivered.
/// </summary>
/// <remarks>
/// A supply order is pure intent: creating, placing, or editing it changes no
/// physical stock. Stock is created only when goods are received against it, at
/// which point the receiving operation effects the stock change (creating
/// <c>Item</c>s or appending an <c>ItemEvent</c>) and then calls
/// <see cref="RegisterReceipt"/> to advance this order's bookkeeping.
///
/// The order carries an optional <see cref="ReceivingContainerGuid"/> — the
/// container into which its goods are received. A <see langword="null"/> value
/// means goods are received at the root, outside any container.
///
/// The order is partition-scoped like every other aggregate (it always belongs
/// to the global partition, which cannot be removed) and participates in
/// optimistic concurrency via <see cref="IConcurrencyVersioned"/>.
/// </remarks>
public sealed class SupplyOrder : Entity, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    /// <summary>
    /// Gets the human-facing reference for the order (for example an order number
    /// or label).
    /// </summary>
    public Name Reference { get; private set; }

    /// <summary>
    /// Gets or sets the description of the order.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the current lifecycle state of the order.
    /// </summary>
    public SupplyOrderStatus Status { get; private set; } = SupplyOrderStatus.Draft;

    /// <summary>
    /// Gets the instant, in UTC, at which the order was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the instant, in UTC, at which the order was placed, or
    /// <see langword="null"/> while the order is still a draft.
    /// </summary>
    public DateTimeOffset? PlacedAt { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the container into which goods on this order
    /// are received.
    /// </summary>
    /// <remarks>
    /// References an <c>Item</c> whose article is a container. A
    /// <see langword="null"/> value means goods are received at the root —
    /// outside any container — mirroring an <c>Item</c> whose
    /// <c>ParentItemContainerGuid</c> is <see langword="null"/>. That the
    /// referenced item is a container and is accessible to the acting principal
    /// is validated by the command handler before it is assigned.
    /// </remarks>
    public Guid? ReceivingContainerGuid { get; private set; }

    /// <summary>
    /// Gets the container item into which goods on this order are received, if
    /// any.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value means goods are received at the root,
    /// outside any container.
    /// </remarks>
    public Item? ReceivingContainer { get; private set; }

    /// <summary>
    /// Gets the lines of the order.
    /// </summary>
    public IReadOnlyCollection<SupplyOrderLine> Lines => lines;

    private readonly List<SupplyOrderLine> lines = [];

    /// <summary>
    /// Gets the partitions directly associated with the order.
    /// </summary>
    /// <remarks>
    /// Every order is always associated with the global partition, which defines
    /// its base partition scope and cannot be removed.
    /// </remarks>
    public IReadOnlyCollection<SupplyOrderPartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions associated with the order.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<SupplyOrderPartition> partitions = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Initializes a new supply order entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private SupplyOrder()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        EntityType = EntityType.SupplyOrder;
    }

    private SupplyOrder(Name reference)
        : this()
    {
        Reference = reference;

        partitions.Add(new SupplyOrderPartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    /// <summary>
    /// Creates a new draft supply order with the specified reference.
    /// </summary>
    /// <param name="reference">The human-facing reference for the order.</param>
    /// <returns>A new <see cref="SupplyOrder"/> in the <see cref="SupplyOrderStatus.Draft"/> state.</returns>
    public static SupplyOrder NewSupplyOrder(Name reference) => new(reference);

    /// <summary>
    /// Gets a value indicating whether the order is currently editable (its lines
    /// may be changed). Only a draft order is editable.
    /// </summary>
    public bool IsEditable => Status == SupplyOrderStatus.Draft;

    /// <summary>
    /// Adds a line for the specified article and ordered quantity. Only valid
    /// while the order is a draft.
    /// </summary>
    /// <param name="article">The article to order.</param>
    /// <param name="orderedQuantity">The strictly positive quantity to order.</param>
    /// <returns>The created line.</returns>
    /// <remarks>
    /// Only event-tracked articles (<see cref="ArticleTrackingMode.Event"/>) may
    /// be ordered: a supply order records aggregate expected quantities, which is
    /// the receiving model for event-tracked stock. Entity-tracked articles are
    /// received as individual items and are not orderable through a supply order.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is not editable, when the article is not event
    /// tracked, when the article is already on the order, or when the quantity is
    /// not strictly positive.
    /// </exception>
    public SupplyOrderLine AddLine(Article article, Scalar orderedQuantity)
    {
        ThrowIfNotEditable();

        ArgumentNullException.ThrowIfNull(article);

        if (article.TrackingMode != ArticleTrackingMode.Event)
        {
            throw new FargoCoreException(
                $"Article '{article.Guid}' is not event tracked and cannot be added to a supply order.",
                FargoErrorType.InvalidOperation);
        }

        if (lines.Any(l => l.ArticleGuid == article.Guid))
        {
            throw new FargoCoreException(
                $"Article '{article.Guid}' is already on this order.",
                FargoErrorType.InvalidOperation);
        }

        var line = new SupplyOrderLine(this, article, orderedQuantity);

        lines.Add(line);

        return line;
    }

    /// <summary>
    /// Changes the ordered quantity of the line for the specified article. Only
    /// valid while the order is a draft.
    /// </summary>
    /// <param name="articleGuid">The article whose line quantity changes.</param>
    /// <param name="orderedQuantity">The new strictly positive ordered quantity.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is not editable, when the article is not on the
    /// order, or when the quantity is not strictly positive.
    /// </exception>
    public void ChangeLineQuantity(Guid articleGuid, Scalar orderedQuantity)
    {
        ThrowIfNotEditable();

        GetLineOrThrow(articleGuid).SetOrderedQuantity(orderedQuantity);
    }

    /// <summary>
    /// Removes the line for the specified article. Only valid while the order is
    /// a draft.
    /// </summary>
    /// <param name="articleGuid">The article whose line is removed.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is not editable or the article is not on the order.
    /// </exception>
    public void RemoveLine(Guid articleGuid)
    {
        ThrowIfNotEditable();

        lines.Remove(GetLineOrThrow(articleGuid));
    }

    /// <summary>
    /// Sets or clears the receiving container. Pass <see langword="null"/> to
    /// receive goods at the root (outside any container).
    /// </summary>
    /// <remarks>
    /// Allowed only while the order is a draft: once the order is placed its
    /// receiving location is fixed, so that goods always arrive where the order
    /// committed to receive them. The caller is responsible for having validated
    /// that <paramref name="container"/> is a container item the acting principal
    /// can access before calling this.
    /// </remarks>
    /// <param name="container">
    /// The container item, or <see langword="null"/> to receive goods at the root.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is not a draft, or when <paramref name="container"/>
    /// is not a container-type article.
    /// </exception>
    public void SetReceivingContainer(Item? container)
    {
        ThrowIfNotEditable();

        if (container is not null && container.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException(
                $"Item '{container.Guid}' is not a container item.",
                FargoErrorType.InvalidOperation);
        }

        ReceivingContainer = container;
        ReceivingContainerGuid = container?.Guid;
    }

    /// <summary>
    /// Commits the order, freezing its lines. Requires at least one line.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is not a draft, or when it has no lines.
    /// </exception>
    public void Place()
    {
        if (Status != SupplyOrderStatus.Draft)
        {
            throw new FargoCoreException(
                $"Only a draft order can be placed (current status: {Status}).",
                FargoErrorType.InvalidOperation);
        }

        if (lines.Count == 0)
        {
            throw new FargoCoreException(
                "A supply order must have at least one line before it can be placed.",
                FargoErrorType.InvalidOperation);
        }

        Status = SupplyOrderStatus.Placed;
        PlacedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Records that a quantity of an article was received against the order,
    /// advancing its status. Over-receipt is permitted, and an already-received
    /// order can still take further receipts (they remain over-receipts and the
    /// order stays <see cref="SupplyOrderStatus.Received"/>).
    /// </summary>
    /// <remarks>
    /// This updates only the order's bookkeeping. The caller must have already
    /// effected the physical stock change (created items or appended an event)
    /// before invoking this method.
    /// </remarks>
    /// <param name="articleGuid">The received article (must be on the order).</param>
    /// <param name="quantity">The strictly positive quantity received.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is not in a receivable state (draft, closed, or
    /// cancelled), when the article is not on the order, or when the quantity is
    /// not strictly positive.
    /// </exception>
    public void RegisterReceipt(Guid articleGuid, Scalar quantity)
    {
        if (Status is not (SupplyOrderStatus.Placed
            or SupplyOrderStatus.PartiallyReceived
            or SupplyOrderStatus.Received))
        {
            throw new FargoCoreException(
                $"Cannot receive against an order with status {Status}.",
                FargoErrorType.InvalidOperation);
        }

        GetLineOrThrow(articleGuid).AddReceived(quantity);

        // An order is Received once every line is fully (or over-) received.
        // IsFullyReceived never regresses once true (received only increases), so
        // receiving against an already-Received order leaves it Received.
        Status = lines.All(l => l.IsFullyReceived)
            ? SupplyOrderStatus.Received
            : SupplyOrderStatus.PartiallyReceived;
    }

    /// <summary>
    /// Cancels the order. Not allowed once the order is received, closed, or
    /// already cancelled.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is in a state that cannot be cancelled.
    /// </exception>
    public void Cancel()
    {
        if (Status is SupplyOrderStatus.Received
            or SupplyOrderStatus.Closed
            or SupplyOrderStatus.Cancelled)
        {
            throw new FargoCoreException(
                $"An order with status {Status} cannot be cancelled.",
                FargoErrorType.InvalidOperation);
        }

        Status = SupplyOrderStatus.Cancelled;
    }

    /// <summary>
    /// Finalizes the order. Allowed once at least some goods have been received.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when the order is in a state that cannot be closed.
    /// </exception>
    public void Close()
    {
        if (Status is not (SupplyOrderStatus.Received or SupplyOrderStatus.PartiallyReceived))
        {
            throw new FargoCoreException(
                $"An order with status {Status} cannot be closed.",
                FargoErrorType.InvalidOperation);
        }

        Status = SupplyOrderStatus.Closed;
    }

    /// <summary>
    /// Associates the order with the specified partition. If the association
    /// already exists, no action is taken.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new SupplyOrderPartition(this, partition));
    }

    /// <summary>
    /// Removes the association between the order and the specified partition.
    /// </summary>
    /// <remarks>
    /// The global partition is mandatory for every order and cannot be removed.
    /// If the order is not associated with the specified partition, no action is
    /// taken.
    /// </remarks>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove the global partition.
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' is mandatory and cannot be removed from a supply order.",
                FargoErrorType.InvalidOperation);
        }

        var partition = partitions.SingleOrDefault(p => p.PartitionGuid == partitionGuid);

        if (partition is not null)
        {
            partitions.Remove(partition);
        }
    }

    private SupplyOrderLine GetLineOrThrow(Guid articleGuid)
        => lines.SingleOrDefault(l => l.ArticleGuid == articleGuid)
            ?? throw new FargoCoreException(
                $"Article '{articleGuid}' is not on this order.",
                FargoErrorType.InvalidOperation);

    private void ThrowIfNotEditable()
    {
        if (!IsEditable)
        {
            throw new FargoCoreException(
                $"The order cannot be modified in status {Status}.",
                FargoErrorType.InvalidOperation);
        }
    }
}
