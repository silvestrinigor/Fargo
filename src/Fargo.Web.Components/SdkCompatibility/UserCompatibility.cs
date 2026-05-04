using Fargo.Api.Partitions;
using Fargo.Sdk.Contracts;

namespace Fargo.Api.Users;

public interface IUserService
{
    Task<User> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);
}

public interface IUserManager : IUserService
{
    Task<IReadOnlyCollection<User>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    Task<User> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);
}

public sealed class User
{
    private readonly IUserHttpClient client;

    internal User(UserResult result, IUserHttpClient client)
    {
        this.client = client;
        Guid = result.Guid;
        Nameid = result.Nameid;
        FirstName = result.FirstName;
        LastName = result.LastName;
        Description = result.Description;
        DefaultPasswordExpirationPeriod = result.DefaultPasswordExpirationPeriod;
        RequirePasswordChangeAt = result.RequirePasswordChangeAt;
        IsActive = result.IsActive;
        Permissions = result.Permissions.Select(x => x.Action).ToArray();
        PartitionAccesses = result.PartitionAccesses.ToArray();
        EditedByGuid = result.EditedByGuid;
    }

    public Guid Guid { get; }

    public string Nameid { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Description { get; set; }

    public TimeSpan DefaultPasswordExpirationPeriod { get; set; }

    public DateTimeOffset RequirePasswordChangeAt { get; }

    public bool IsActive { get; set; }

    public IReadOnlyCollection<ActionType> Permissions { get; set; }

    public IReadOnlyCollection<Guid> PartitionAccesses { get; set; }

    public Guid? EditedByGuid { get; }

    public async Task UpdateAsync(Action<User> update, CancellationToken cancellationToken = default)
    {
        update(this);
        (await client.UpdateAsync(
            Guid,
            Nameid,
            FirstName,
            LastName,
            Description,
            isActive: IsActive,
            permissions: Permissions,
            defaultPasswordExpirationPeriod: DefaultPasswordExpirationPeriod,
            cancellationToken: cancellationToken)).EnsureSuccess("Failed to update user.");
    }

    public Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
        => client.AddUserGroupAsync(Guid, userGroupGuid, cancellationToken);

    public Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
        => client.RemoveUserGroupAsync(Guid, userGroupGuid, cancellationToken);

    public Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
        => client.AddPartitionAsync(Guid, partitionGuid, cancellationToken);

    public Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
        => client.RemovePartitionAsync(Guid, partitionGuid, cancellationToken);

    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);
}

public sealed class UserManager(IUserHttpClient client) : IUserManager
{
    public async Task<User> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => new((await client.GetAsync(userGuid, temporalAsOf, cancellationToken)).Unwrap("Failed to load user."), client);

    public async Task<IReadOnlyCollection<User>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default)
        => (await client.GetManyAsync(temporalAsOf, page, limit, partitionGuid, search, noPartition, cancellationToken))
            .Unwrap("Failed to load users.")
            .Select(x => new User(x, client))
            .ToArray();

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
        var guid = (await client.CreateAsync(
            nameid,
            password,
            firstName,
            lastName,
            description,
            permissions,
            defaultPasswordExpirationPeriod,
            firstPartition,
            cancellationToken)).Unwrap("Failed to create user.");

        return await GetAsync(guid, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
        => (await client.DeleteAsync(userGuid, cancellationToken)).EnsureSuccess("Failed to delete user.");
}
