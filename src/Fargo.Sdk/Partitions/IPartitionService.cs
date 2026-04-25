namespace Fargo.Sdk.Partitions;

/// <summary>Provides CRUD operations for partitions and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IPartitionService
{
    Task<Partition> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Partition>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default);

    Task<Partition> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
