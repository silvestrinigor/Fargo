using Fargo.Sdk.Models;

namespace Fargo.Sdk.Managers;

public interface IPartitionManager
{
    Task<PartitionInfo?> GetAsync(Guid partitionGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<PartitionInfo>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken ct = default);

    Task UpdateAsync(Guid partitionGuid, string? description = null, Guid? parentPartitionGuid = null, CancellationToken ct = default);

    Task DeleteAsync(Guid partitionGuid, CancellationToken ct = default);
}
