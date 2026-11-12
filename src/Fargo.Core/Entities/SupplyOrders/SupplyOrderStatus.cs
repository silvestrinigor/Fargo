namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// Lifecycle of a <see cref="SupplyOrder"/>: Draft, then Placed, then PartiallyReceived or Received,
/// then Closed. An order can be Cancelled until it is received or closed.
/// </summary>
public enum SupplyOrderStatus : byte
{
    /// <summary>
    /// Editable and not yet placed. Lines may be added, changed, or removed only
    /// in this state.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Committed. Lines are frozen; the order is awaiting goods.
    /// </summary>
    Placed = 2,

    /// <summary>
    /// Some, but not all, of the ordered quantity has been received.
    /// </summary>
    PartiallyReceived = 3,

    /// <summary>
    /// The full ordered quantity (or more) has been received on every line.
    /// </summary>
    Received = 4,

    /// <summary>
    /// Finalized. No further receiving is allowed.
    /// </summary>
    Closed = 5,

    /// <summary>
    /// Abandoned before full receipt. No further receiving is allowed.
    /// </summary>
    Cancelled = 6
}
