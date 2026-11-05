namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// Defines the repository contract for querying and persisting
/// <see cref="SupplyOrder"/> entities.
/// </summary>
/// <remarks>
/// Implementations retrieve supply orders from the persistence layer and track
/// changes for creation, mutation, and deletion. Changes are committed through a
/// unit of work.
/// </remarks>
public interface ISupplyOrderRepository
{
    /// <summary>
    /// Gets a supply order by its unique identifier, including its lines (and
    /// each line's article), its partitions, and its receiving container.
    /// </summary>
    /// <param name="supplyOrderGuid">The unique identifier of the supply order.</param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="SupplyOrder"/> if found; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    Task<SupplyOrder?> GetByGuidAsync(Guid supplyOrderGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new supply order to the persistence context.
    /// </summary>
    /// <param name="supplyOrder">The supply order to add.</param>
    /// <remarks>
    /// The supply order is tracked by the persistence context. The operation is
    /// not committed until the associated unit of work is completed.
    /// </remarks>
    void Add(SupplyOrder supplyOrder);

    /// <summary>
    /// Removes a supply order from the persistence context.
    /// </summary>
    /// <param name="supplyOrder">The supply order to remove.</param>
    /// <remarks>
    /// The removal is staged in the persistence context and is not committed
    /// until the associated unit of work is completed.
    /// </remarks>
    void Remove(SupplyOrder supplyOrder);
}
