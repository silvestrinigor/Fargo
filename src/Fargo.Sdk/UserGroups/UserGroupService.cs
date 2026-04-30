using Fargo.Sdk.Events;
using System.Collections.Concurrent;

namespace Fargo.Sdk.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupService"/>.</summary>
public sealed class UserGroupService : IUserGroupService
{
    /// <summary>Initializes a new instance.</summary>
    public UserGroupService(IUserGroupHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnUserGroupUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var userGroup))
            {
                userGroup.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnUserGroupDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var userGroup))
            {
                userGroup.RaiseDeleted();
            }
        });

        this.hub = hub;
    }

    private readonly ConcurrentDictionary<Guid, UserGroup> tracked = new();
    private readonly IUserGroupHttpClient client;
    private readonly IFargoEventHub hub;

    /// <inheritdoc />
    public async Task<UserGroup> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(userGroupGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<UserGroup>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public async Task<UserGroup> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(nameid, description, permissions, firstPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var userGroup = new UserGroup(response.Data, nameid, description ?? string.Empty, true, (permissions ?? []).ToList(), client, MakeDisposeCallback(response.Data));
        return await TrackAsync(userGroup);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(userGroupGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<UserGroup> ToEntityAsync(UserGroupResult r)
    {
        var userGroup = new UserGroup(r.Guid, r.Nameid, r.Description, r.IsActive, r.Permissions.Select(p => p.Action).ToList(), client, MakeDisposeCallback(r.Guid));
        return await TrackAsync(userGroup);
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.TryRemove(guid, out _);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private async Task<UserGroup> TrackAsync(UserGroup userGroup)
    {
        var trackedUserGroup = tracked.GetOrAdd(userGroup.Guid, userGroup);

        if (ReferenceEquals(trackedUserGroup, userGroup))
        {
            await hub.InvokeAsync("SubscribeToEntityAsync", userGroup.Guid);
        }

        return trackedUserGroup;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error);
}
