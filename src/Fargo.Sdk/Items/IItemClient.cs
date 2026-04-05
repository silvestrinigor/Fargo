using Fargo.Sdk.Models;

namespace Fargo.Sdk.Items;

internal interface IItemClient
{
    Task<ItemInfo?> GetAsync(Guid itemGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<ItemInfo>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken ct = default);

    Task UpdateAsync(Guid itemGuid, CancellationToken ct = default);

    Task DeleteAsync(Guid itemGuid, CancellationToken ct = default);
}
