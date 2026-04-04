using Fargo.Sdk.Managers;
using Fargo.Sdk.Models;

namespace Fargo.Sdk;

public interface IClient
{
    event EventHandler<LoggedInEventArgs>? LoggedIn;

    event EventHandler<LoggedOutEventArgs>? LoggedOut;

    bool IsConnected { get; }

    IUserManager Users { get; }

    IArticleManager Articles { get; }

    IItemManager Items { get; }

    IPartitionManager Partitions { get; }

    IUserGroupManager UserGroups { get; }

    ITreeManager Trees { get; }

    Task LogInAsync(string server, string nameid, string password, CancellationToken ct = default);

    Task LogOutAsync(CancellationToken ct = default);

    Task<AuthResult> RefreshAsync(CancellationToken ct = default);

    Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken ct = default);
}
