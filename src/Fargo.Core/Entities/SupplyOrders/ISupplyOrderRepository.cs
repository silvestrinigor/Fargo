namespace Fargo.Core.Entities.SupplyOrders;

public interface ISupplyOrderRepository
{
    /// <summary>
    /// Loaded with its lines (and their articles), its partitions and its receiving container.
    /// </summary>
    Task<SupplyOrder?> GetByGuidAsync(Guid supplyOrderGuid, CancellationToken cancellationToken = default);

    void Add(SupplyOrder supplyOrder);

    void Remove(SupplyOrder supplyOrder);
}
