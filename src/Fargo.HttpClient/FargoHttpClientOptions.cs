namespace Fargo.HttpClient;

public sealed class FargoHttpClientOptions
{
    public Uri? BaseAddress { get; set; }

    public string? BearerToken { get; set; }

    public Func<CancellationToken, ValueTask<string?>>? BearerTokenProvider { get; set; }
}
