using Fargo.Sdk.Events;

namespace Fargo.Sdk.Users;

/// <summary>Default implementation of <see cref="IUserManager"/>.</summary>
public sealed class UserManager : IUserManager
{
    internal UserManager(IUserClient client, FargoHubConnection hub)
    {
        this.client = client;
        this.hub = hub;

        hub.On<Guid, string>("OnUserCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserCreatedEventArgs(guid, nameid)));

        hub.On<Guid>("OnUserUpdated", guid =>
        {
            if (_tracked.TryGetValue(guid, out var user))
            {
                user.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnUserDeleted", guid =>
        {
            if (_tracked.TryGetValue(guid, out var user))
            {
                user.RaiseDeleted();
            }
        });
    }

    public event EventHandler<UserCreatedEventArgs>? Created;

    private readonly Dictionary<Guid, User> _tracked = new();
    private readonly IUserClient client;
    private readonly FargoHubConnection hub;

    public async Task<User> GetAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(userGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    public async Task<IReadOnlyCollection<User>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(temporalAsOf, page, limit, cancellationToken);

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

    public async Task<User> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var createResponse = await client.CreateAsync(
            nameid, password, firstName, lastName, description,
            permissions, defaultPasswordExpirationPeriod, firstPartition, cancellationToken);

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

    public async Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(userGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<User> ToEntityAsync(UserResult r)
    {
        var user = new User(
            r.Guid,
            r.Nameid,
            r.FirstName,
            r.LastName,
            r.Description,
            r.DefaultPasswordExpirationPeriod,
            r.RequirePasswordChangeAt,
            r.IsActive,
            r.Permissions.Select(p => p.Action).ToList(),
            r.PartitionAccesses,
            client,
            MakeDisposeCallback(r.Guid));
        _tracked[user.Guid] = user;
        await hub.InvokeAsync("SubscribeToEntityAsync", user.Guid);
        return user;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        _tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
