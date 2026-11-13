namespace Fargo.Client.Realtime;

/// <summary>
/// Provides the access token the <see cref="FargoRealtimeClient"/> sends when it connects to the hub.
/// </summary>
/// <remarks>
/// The client asks for a token on every connect and reconnect, and the service closes a connection
/// when its token expires. Obtaining and refreshing tokens is the application's job, for example
/// through the Fargo HTTP client's token source.
/// </remarks>
public interface IFargoRealtimeAccessTokenProvider
{
    /// <summary>
    /// Gets the access token to send, or <see langword="null"/> to connect without one.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// An <see cref="IFargoRealtimeAccessTokenProvider"/> that calls a delegate, for example
/// <c>new DelegateRealtimeAccessTokenProvider(tokenSource.GetAccessTokenAsync)</c>.
/// </summary>
public sealed class DelegateRealtimeAccessTokenProvider(Func<CancellationToken, Task<string?>> getAccessTokenAsync)
    : IFargoRealtimeAccessTokenProvider
{
    private readonly Func<CancellationToken, Task<string?>> getAccessTokenAsync =
        getAccessTokenAsync ?? throw new ArgumentNullException(nameof(getAccessTokenAsync));

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        => getAccessTokenAsync(cancellationToken);
}
