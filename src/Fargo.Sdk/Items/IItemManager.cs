namespace Fargo.Sdk.Items;

public interface IItemManager
{
    Task<Item?> GetItemAsync(Guid itemGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<Item>> GetItemsAsync(Guid? articleGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateItemAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken ct = default);

    Task UpdateItemAsync(Guid itemGuid, CancellationToken ct = default);

    Task DeleteItemAsync(Guid itemGuid, CancellationToken ct = default);
}
