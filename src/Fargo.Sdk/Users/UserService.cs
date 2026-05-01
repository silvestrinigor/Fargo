using Fargo.Sdk.Events;
using System.Collections.Concurrent;

namespace Fargo.Sdk.Users;

/// <summary>Default implementation of <see cref="IUserService"/>.</summary>
public sealed class UserService : IUserService
{
    /// <summary>Initializes a new instance.</summary>
    public UserService(IUserHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnUserUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var user))
            {
                user.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnUserDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var user))
            {
                user.RaiseDeleted();
            }
        });

        this.hub = hub;
    }

    private readonly ConcurrentDictionary<Guid, User> tracked = new();
    private readonly IUserHttpClient client;
    private readonly IFargoEventHub hub;

    /// <inheritdoc />
    public async Task<User> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(userGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<User>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(temporalAsOf, page, limit, partitionGuid, search, noPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var entities = new List<User>();
        foreach (var r in response.Data!)
        {
            entities.Add(await ToEntityAsync(r));
        }

        return entities;
    }

    /// <inheritdoc />
    public async Task<User> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
    {
        var createResponse = await client.CreateAsync(nameid, password, firstName, lastName, description, permissions, defaultPasswordExpirationPeriod, firstPartition, cancellationToken);

        if (!createResponse.IsSuccess)
        {
            ThrowError(createResponse.Error!);
        }

        var getResponse = await client.GetAsync(createResponse.Data, cancellationToken: cancellationToken);

        if (!getResponse.IsSuccess)
        {
            ThrowError(getResponse.Error!);
        }

        return await ToEntityAsync(getResponse.Data!);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(userGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<User> ToEntityAsync(UserResult r)
    {
        var user = new User(r.Guid, r.Nameid, r.FirstName, r.LastName, r.Description, r.DefaultPasswordExpirationPeriod, r.RequirePasswordChangeAt, r.IsActive, r.Permissions.Select(p => p.Action).ToList(), r.PartitionAccesses, client, MakeDisposeCallback(r.Guid), r.EditedByGuid);
        return await TrackAsync(user);
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.TryRemove(guid, out _);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private async Task<User> TrackAsync(User user)
    {
        var trackedUser = tracked.GetOrAdd(user.Guid, user);

        if (ReferenceEquals(trackedUser, user))
        {
            await hub.InvokeAsync("SubscribeToEntityAsync", user.Guid);
        }

        return trackedUser;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error);
}
