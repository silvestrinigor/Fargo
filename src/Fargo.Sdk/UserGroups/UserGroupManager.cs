using Fargo.Sdk.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupManager"/>.</summary>
public sealed class UserGroupManager : IUserGroupManager
{
    internal UserGroupManager(IUserGroupClient client, FargoHubConnection hub, ILogger logger)
    {
        this.client = client;
        this.logger = logger;

        hub.On<Guid, string>("OnUserGroupCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserGroupCreatedEventArgs(guid, nameid)));

        hub.On<Guid>("OnUserGroupUpdated", guid =>
            Updated?.Invoke(this, new UserGroupUpdatedEventArgs(guid)));

        hub.On<Guid>("OnUserGroupDeleted", guid =>
            Deleted?.Invoke(this, new UserGroupDeletedEventArgs(guid)));
    }

    public event EventHandler<UserGroupCreatedEventArgs>? Created;
    public event EventHandler<UserGroupUpdatedEventArgs>? Updated;
    public event EventHandler<UserGroupDeletedEventArgs>? Deleted;

    private readonly IUserGroupClient client;
    private readonly ILogger logger;

    public async Task<UserGroup> GetAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(userGroupGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return ToEntity(response.Data!);
    }

    public async Task<IReadOnlyCollection<UserGroup>> GetManyAsync(
        Guid? userGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(userGuid, temporalAsOf, page, limit, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data!.Select(ToEntity).ToList();
    }

    public async Task<UserGroup> CreateAsync(
        string nameid,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(nameid, description, permissions, firstPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return new UserGroup(
            response.Data,
            nameid,
            description ?? string.Empty,
            true,
            (permissions ?? []).ToList(),
            client,
            logger);
    }

    public async Task DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(userGroupGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private UserGroup ToEntity(UserGroupResult r) => new(
        r.Guid,
        r.Nameid,
        r.Description,
        r.IsActive,
        r.Permissions.Select(p => p.Action).ToList(),
        client,
        logger);

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
