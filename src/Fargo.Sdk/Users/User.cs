using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Users;

/// <summary>
/// Represents a live user entity. Call <see cref="UpdateAsync"/> to persist property changes.
/// </summary>
public sealed class User
{
    internal User(
        Guid guid,
        string nameid,
        string? firstName,
        string? lastName,
        string description,
        TimeSpan defaultPasswordExpirationPeriod,
        DateTimeOffset requirePasswordChangeAt,
        bool isActive,
        IReadOnlyCollection<ActionType> permissions,
        IReadOnlyCollection<Guid> partitionAccesses,
        IUserClient client)
    {
        Guid = guid;
        _nameid = nameid;
        _firstName = firstName;
        _lastName = lastName;
        _description = description;
        _isActive = isActive;
        DefaultPasswordExpirationPeriod = defaultPasswordExpirationPeriod;
        RequirePasswordChangeAt = requirePasswordChangeAt;
        Permissions = permissions;
        PartitionAccesses = partitionAccesses;
        this.client = client;
    }

    private readonly IUserClient client;

    /// <summary>The unique identifier of the user.</summary>
    public Guid Guid { get; }

    private string _nameid;

    /// <summary>The login name identifier.</summary>
    public string Nameid
    {
        get => _nameid;
        set => _nameid = value;
    }

    private string? _firstName;

    /// <summary>The user's first name.</summary>
    public string? FirstName
    {
        get => _firstName;
        set => _firstName = value;
    }

    private string? _lastName;

    /// <summary>The user's last name.</summary>
    public string? LastName
    {
        get => _lastName;
        set => _lastName = value;
    }

    private string _description;

    /// <summary>The description of the user.</summary>
    public string Description
    {
        get => _description;
        set => _description = value;
    }

    private bool _isActive;

    /// <summary>Whether the user account is active.</summary>
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    /// <summary>Raised when this user is updated by any authenticated client.</summary>
    public event EventHandler<UserUpdatedEventArgs>? Updated;

    /// <summary>Raised when this user is deleted by any authenticated client.</summary>
    public event EventHandler<UserDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new UserUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new UserDeletedEventArgs(Guid));

    /// <summary>The period after which the user's password expires.</summary>
    public TimeSpan DefaultPasswordExpirationPeriod { get; }

    /// <summary>The point in time at which the user is required to change their password.</summary>
    public DateTimeOffset RequirePasswordChangeAt { get; }

    /// <summary>The permissions assigned directly to this user.</summary>
    public IReadOnlyCollection<ActionType> Permissions { get; }

    /// <summary>The identifiers of partitions this user can access.</summary>
    public IReadOnlyCollection<Guid> PartitionAccesses { get; }

    /// <summary>Gets the partitions that directly contain this user.</summary>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    /// <summary>Adds this user to a user group.</summary>
    public Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
        => client.AddUserGroupAsync(Guid, userGroupGuid, cancellationToken);

    /// <summary>Removes this user from a user group.</summary>
    public Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
        => client.RemoveUserGroupAsync(Guid, userGroupGuid, cancellationToken);

    /// <summary>
    /// Applies <paramref name="update"/> to this user and persists all changes in a single request.
    /// </summary>
    /// <param name="update">An action that sets one or more properties on this user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the update fails.</exception>
    public async Task UpdateAsync(Action<User> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var result = await client.UpdateAsync(Guid, _nameid, _firstName, _lastName, _description, null, _isActive, null, null, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
    }
}
