using Microsoft.Kiota.Abstractions;

namespace Fargo.ClientHttp;

/// <summary>
/// Sends an <c>Idempotency-Key</c> header, which makes a request safe to retry on the endpoints that support it.
/// </summary>
/// <remarks>
/// A retry with the same key returns the first result instead of repeating the operation. The retry handler of
/// the client resends the same request, so the key it was given on the first attempt is kept. Use a new key for a
/// different request.
/// </remarks>
public static class IdempotencyKeys
{
    public const string HeaderName = "Idempotency-Key";

    /// <summary>
    /// A request configuration that adds the header, to pass as the configuration of a generated request.
    /// </summary>
    public static Action<RequestConfiguration<DefaultQueryParameters>> Header(Guid key)
        => request => request.Headers.TryAdd(HeaderName, key.ToString());
}
