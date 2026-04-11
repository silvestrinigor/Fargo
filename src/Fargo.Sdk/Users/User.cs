using Fargo.Sdk.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Users;

/// <summary>
/// Represents a live user entity. Setting <see cref="Nameid"/>, <see cref="FirstName"/>,
/// <see cref="LastName"/>, <see cref="Description"/>, or <see cref="IsActive"/> automatically
/// sends a PATCH request to the backend to persist the change.
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
        IUserClient client,
        ILogger logger)
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
        this.logger = logger;
    }

    private readonly IUserClient client;
    private readonly ILogger logger;

    /// <summary>The unique identifier of the user.</summary>
    public Guid Guid { get; }

    private string _nameid;

    /// <summary>
    /// The login name identifier. Setting this property fires a background update request.
    /// </summary>
    public string Nameid
    {
        get => _nameid;
        set
        {
            if (_nameid == value)
            {
                return;
            }

            _nameid = value;
            _ = SendUpdateAsync();
        }
    }

    private string? _firstName;

    /// <summary>
    /// The user's first name. Setting this property fires a background update request.
    /// </summary>
    public string? FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName == value)
            {
                return;
            }

            _firstName = value;
            _ = SendUpdateAsync();
        }
    }

    private string? _lastName;

    /// <summary>
    /// The user's last name. Setting this property fires a background update request.
    /// </summary>
    public string? LastName
    {
        get => _lastName;
        set
        {
            if (_lastName == value)
            {
                return;
            }

            _lastName = value;
            _ = SendUpdateAsync();
        }
    }

    private string _description;

    /// <summary>
    /// The description of the user. Setting this property fires a background update request.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description == value)
            {
                return;
            }

            _description = value;
            _ = SendUpdateAsync();
        }
    }

    private bool _isActive;

    /// <summary>
    /// Whether the user account is active. Setting this property fires a background update request.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            _isActive = value;
            _ = SendUpdateAsync();
        }
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

    private async Task SendUpdateAsync()
    {
        var result = await client.UpdateAsync(Guid, _nameid, _firstName, _lastName, _description, null, _isActive);

        if (!result.IsSuccess)
        {
            logger.LogUserUpdateFailed(Guid, result.Error!.Detail);
        }
    }
}
