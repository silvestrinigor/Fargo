namespace Fargo.Sdk.Items;

/// <summary>Provides CRUD operations for items and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IItemService
{
    Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Item>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}
