using Fargo.Sdk.Events;

namespace Fargo.Sdk.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupManager"/>.</summary>
public sealed class UserGroupManager : IUserGroupManager
{
    internal UserGroupManager(IUserGroupClient client, FargoHubConnection hub)
    {
        this.client = client;
        this.hub = hub;

        hub.On<Guid, string>("OnUserGroupCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserGroupCreatedEventArgs(guid, nameid)));

        hub.On<Guid>("OnUserGroupUpdated", guid =>
        {
            if (_tracked.TryGetValue(guid, out var userGroup))
            {
                userGroup.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnUserGroupDeleted", guid =>
        {
            if (_tracked.TryGetValue(guid, out var userGroup))
            {
                userGroup.RaiseDeleted();
            }
        });
    }

    public event EventHandler<UserGroupCreatedEventArgs>? Created;

    private readonly Dictionary<Guid, UserGroup> _tracked = new();
    private readonly IUserGroupClient client;
    private readonly FargoHubConnection hub;

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

        return await ToEntityAsync(response.Data!);
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

        var entities = new List<UserGroup>();
        foreach (var r in response.Data!)
        {
            entities.Add(await ToEntityAsync(r));
        }

        return entities;
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

        var userGroup = new UserGroup(
            response.Data,
            nameid,
            description ?? string.Empty,
            true,
            (permissions ?? []).ToList(),
            client,
            MakeDisposeCallback(response.Data));
        _tracked[userGroup.Guid] = userGroup;
        await hub.InvokeAsync("SubscribeToEntityAsync", userGroup.Guid);
        return userGroup;
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

    private async Task<UserGroup> ToEntityAsync(UserGroupResult r)
    {
        var userGroup = new UserGroup(
            r.Guid,
            r.Nameid,
            r.Description,
            r.IsActive,
            r.Permissions.Select(p => p.Action).ToList(),
            client,
            MakeDisposeCallback(r.Guid));
        _tracked[userGroup.Guid] = userGroup;
        await hub.InvokeAsync("SubscribeToEntityAsync", userGroup.Guid);
        return userGroup;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        _tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
