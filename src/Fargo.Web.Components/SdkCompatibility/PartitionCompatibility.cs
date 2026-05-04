namespace Fargo.Api.Partitions;

public interface IPartitionManager
{
    Task<Partition> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Partition>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        bool? rootOnly = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<Partition> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}

public sealed class Partition
{
    private readonly IPartitionHttpClient client;

    internal Partition(PartitionResult result, IPartitionHttpClient client)
    {
        this.client = client;
        Guid = result.Guid;
        Name = result.Name;
        Description = result.Description;
        ParentPartitionGuid = result.ParentPartitionGuid;
        IsActive = result.IsActive;
        EditedByGuid = result.EditedByGuid;
    }

    public Guid Guid { get; }

    public string Name { get; set; }

    public string Description { get; set; }

    public Guid? ParentPartitionGuid { get; set; }

    public bool IsActive { get; set; }

    public Guid? EditedByGuid { get; }

    public async Task UpdateAsync(Action<Partition> update, CancellationToken cancellationToken = default)
    {
        update(this);
        (await client.UpdateAsync(Guid, Name, Description, ParentPartitionGuid, IsActive, cancellationToken))
            .EnsureSuccess("Failed to update partition.");
    }

    public async Task<FargoSdkResponse<EmptyResult>> MoveAsync(Guid parentPartitionGuid, CancellationToken cancellationToken = default)
    {
        ParentPartitionGuid = parentPartitionGuid;
        return await client.UpdateAsync(Guid, parentPartitionGuid: parentPartitionGuid, cancellationToken: cancellationToken);
    }
}

public sealed class PartitionManager(IPartitionHttpClient client) : IPartitionManager
{
    public async Task<Partition> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => new((await client.GetAsync(partitionGuid, temporalAsOf, cancellationToken)).Unwrap("Failed to load partition."), client);

    public async Task<IReadOnlyCollection<Partition>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        bool? rootOnly = null,
        string? search = null,
        CancellationToken cancellationToken = default)
        => (await client.GetManyAsync(parentPartitionGuid, temporalAsOf, page, limit, rootOnly, search, cancellationToken))
            .Unwrap("Failed to load partitions.")
            .Select(x => new Partition(x, client))
            .ToArray();

    public async Task<Partition> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default)
    {
        var guid = (await client.CreateAsync(name, description, parentPartitionGuid, cancellationToken)).Unwrap("Failed to create partition.");
        return await GetAsync(guid, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
        => (await client.DeleteAsync(partitionGuid, cancellationToken)).EnsureSuccess("Failed to delete partition.");
}
