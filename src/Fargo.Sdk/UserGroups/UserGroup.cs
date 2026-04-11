using Fargo.Sdk.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.UserGroups;

/// <summary>
/// Represents a live user group entity. Setting <see cref="Nameid"/>, <see cref="Description"/>,
/// or <see cref="IsActive"/> automatically sends a PATCH request to the backend to persist the change.
/// </summary>
public sealed class UserGroup
{
    internal UserGroup(
        Guid guid,
        string nameid,
        string description,
        bool isActive,
        IReadOnlyCollection<ActionType> permissions,
        IUserGroupClient client,
        ILogger logger)
    {
        Guid = guid;
        _nameid = nameid;
        _description = description;
        _isActive = isActive;
        Permissions = permissions;
        this.client = client;
        this.logger = logger;
    }

    private readonly IUserGroupClient client;
    private readonly ILogger logger;

    /// <summary>The unique identifier of the user group.</summary>
    public Guid Guid { get; }

    private string _nameid;

    /// <summary>
    /// The name identifier of the user group. Setting this property fires a background update request.
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

    private string _description;

    /// <summary>
    /// The description of the user group. Setting this property fires a background update request.
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
    /// Whether the user group is active. Setting this property fires a background update request.
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

    /// <summary>The permissions assigned to this user group.</summary>
    public IReadOnlyCollection<ActionType> Permissions { get; }

    /// <summary>Gets the partitions that directly contain this user group.</summary>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    private async Task SendUpdateAsync()
    {
        var result = await client.UpdateAsync(Guid, _nameid, _description, _isActive);

        if (!result.IsSuccess)
        {
            logger.LogUserGroupUpdateFailed(Guid, result.Error!.Detail);
        }
    }
}
