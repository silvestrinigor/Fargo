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
/// A supply order is pure intent: creating, placing or editing it changes no physical stock. Stock
/// is created only when goods are received against it; the receiving operation effects the stock
/// change (creating <c>Item</c>s or appending an <c>ItemEvent</c>) and then calls
/// <see cref="RegisterReceipt"/> to advance this order's bookkeeping.
///
/// It always belongs to the global partition, which cannot be removed.
/// </remarks>
public sealed class SupplyOrder : Entity, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    /// <summary>
    /// The human-facing reference, for example an order number or label.
    /// </summary>
    public Name Reference { get; private set; }

    public Description Description { get; set; } = Description.Empty;

    public SupplyOrderStatus Status { get; private set; } = SupplyOrderStatus.Draft;

    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// <see langword="null"/> while the order is still a draft.
    /// </summary>
    public DateTimeOffset? PlacedAt { get; private set; }

    /// <summary>
    /// The container item into which goods are received; <see langword="null"/> means the root,
    /// outside any container.
    /// </summary>
    /// <remarks>
    /// The command handler validates that the item is a container accessible to the acting principal
    /// before it is assigned.
    /// </remarks>
    public Guid? ReceivingContainerGuid { get; private set; }

    public Item? ReceivingContainer { get; private set; }

    public IReadOnlyCollection<SupplyOrderLine> Lines => lines;

    private readonly List<SupplyOrderLine> lines = [];

    public IReadOnlyCollection<SupplyOrderPartition> Partitions => partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<SupplyOrderPartition> partitions = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Required by EF Core.
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

    public static SupplyOrder NewSupplyOrder(Name reference) => new(reference);

    /// <summary>
    /// Whether the lines and receiving container may be changed; only a draft order is editable.
    /// </summary>
    public bool IsEditable => Status == SupplyOrderStatus.Draft;

    /// <summary>
    /// Only valid while the order is a draft.
    /// </summary>
    /// <remarks>
    /// Only event-tracked articles (<see cref="ArticleTrackingMode.Event"/>) may
    /// be ordered: a supply order records aggregate expected quantities, which is
    /// the receiving model for event-tracked stock. Entity-tracked articles are
    /// received as individual items and are not orderable through a supply order.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// The order is not editable, the article is not event tracked or is already on the order, or the
    /// quantity is not strictly positive.
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
    /// Only valid while the order is a draft.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The order is not editable, the article is not on the order, or the quantity is not strictly
    /// positive.
    /// </exception>
    public void ChangeLineQuantity(Guid articleGuid, Scalar orderedQuantity)
    {
        ThrowIfNotEditable();

        GetLineOrThrow(articleGuid).SetOrderedQuantity(orderedQuantity);
    }

    /// <summary>
    /// Only valid while the order is a draft.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The order is not editable or the article is not on the order.
    /// </exception>
    public void RemoveLine(Guid articleGuid)
    {
        ThrowIfNotEditable();

        lines.Remove(GetLineOrThrow(articleGuid));
    }

    /// <summary>
    /// Pass <see langword="null"/> to receive goods at the root.
    /// </summary>
    /// <remarks>
    /// Allowed only while the order is a draft: once placed, the receiving location is fixed so goods
    /// arrive where the order committed to receive them. The caller must have validated that the
    /// container is accessible to the acting principal.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// The order is not a draft, or the item is not a container-type article.
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
    /// The order is not a draft or has no lines.
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
    /// advancing its status. Over-receipt is permitted, and a received order still accepts further
    /// receipts (it stays <see cref="SupplyOrderStatus.Received"/>).
    /// </summary>
    /// <remarks>
    /// This updates only the order's bookkeeping. The caller must have already
    /// effected the physical stock change (created items or appended an event)
    /// before invoking this method.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// The order is draft, closed or cancelled, the article is not on the order, or the quantity is
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
    /// <exception cref="FargoCoreException">The order is not in a cancellable state.</exception>
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
    /// <exception cref="FargoCoreException">The order is not in a closable state.</exception>
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
    /// Does nothing if the order is already associated with the partition.
    /// </summary>
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
    /// Does nothing if the order is not associated with the partition.
    /// </summary>
    /// <exception cref="FargoCoreException">The partition is the global partition.</exception>
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
