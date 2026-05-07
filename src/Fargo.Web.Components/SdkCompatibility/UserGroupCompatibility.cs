using Fargo.Sdk.Contracts;
using Fargo.Sdk.Contracts.UserGroups;

namespace Fargo.Sdk.UserGroups;

public interface IUserGroupManager
{
    Task<UserGroup> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserGroup>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default);

    Task<UserGroup> CreateAsync(
        string nameid,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);
}

public sealed class UserGroup
{
    private readonly IUserGroupHttpClient client;

    internal UserGroup(UserGroupInfo result, IUserGroupHttpClient client)
    {
        this.client = client;
        Guid = result.Guid;
        Nameid = result.Nameid;
        Description = result.Description;
        IsActive = result.IsActive;
        Permissions = result.Permissions.Select(x => x.Action).ToArray();
        Partitions = result.Partitions.ToArray();
    }

    public Guid Guid { get; }

    public string Nameid { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyCollection<ActionType> Permissions { get; set; }

    public IReadOnlyCollection<Guid> Partitions { get; set; }

    public async Task UpdateAsync(Action<UserGroup> update, CancellationToken cancellationToken = default)
    {
        update(this);
        (await client.UpdateAsync(Guid, Nameid, Description, IsActive, Permissions, cancellationToken))
            .EnsureSuccess("Failed to update user group.");
    }
}

public sealed class UserGroupManager(IUserGroupHttpClient client) : IUserGroupManager
{
    public async Task<UserGroup> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => new((await client.GetAsync(userGroupGuid, temporalAsOf, cancellationToken)).Unwrap("Failed to load user group."), client);

    public async Task<IReadOnlyCollection<UserGroup>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
        => (await client.GetManyAsync(temporalAsOf, page, limit, insideAnyOfThisPartitions, notInsideAnyPartition, cancellationToken))
            .Unwrap("Failed to load user groups.")
            .Select(x => new UserGroup(x, client))
            .ToArray();

    public async Task<UserGroup> CreateAsync(
        string nameid,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var guid = (await client.CreateAsync(nameid, description, permissions, firstPartition, cancellationToken))
            .Unwrap("Failed to create user group.");

        return await GetAsync(guid, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
        => (await client.DeleteAsync(userGroupGuid, cancellationToken)).EnsureSuccess("Failed to delete user group.");
}
