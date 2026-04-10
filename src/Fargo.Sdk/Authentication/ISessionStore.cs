namespace Fargo.Sdk.Authentication;

public interface ISessionStore
{
    Task<StoredSession?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(StoredSession session, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
