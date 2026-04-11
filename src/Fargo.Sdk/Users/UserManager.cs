using Fargo.Sdk.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Users;

/// <summary>Default implementation of <see cref="IUserManager"/>.</summary>
public sealed class UserManager : IUserManager
{
    internal UserManager(IUserClient client, FargoHubConnection hub, ILogger logger)
    {
        this.client = client;
        this.logger = logger;

        hub.On<Guid, string>("OnUserCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserCreatedEventArgs(guid, nameid)));

        hub.On<Guid>("OnUserUpdated", guid =>
            Updated?.Invoke(this, new UserUpdatedEventArgs(guid)));

        hub.On<Guid>("OnUserDeleted", guid =>
            Deleted?.Invoke(this, new UserDeletedEventArgs(guid)));
    }

    public event EventHandler<UserCreatedEventArgs>? Created;
    public event EventHandler<UserUpdatedEventArgs>? Updated;
    public event EventHandler<UserDeletedEventArgs>? Deleted;

    private readonly IUserClient client;
    private readonly ILogger logger;

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

        return ToEntity(response.Data!);
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

        return response.Data!.Select(ToEntity).ToList();
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

        return ToEntity(getResponse.Data!);
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

    private User ToEntity(UserResult r) => new(
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
        logger);

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
