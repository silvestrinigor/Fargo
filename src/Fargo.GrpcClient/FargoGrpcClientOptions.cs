namespace Fargo.GrpcClient;

public sealed class FargoGrpcClientOptions
{
    public Uri? Address { get; set; }

    public TimeSpan? DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public Func<CancellationToken, ValueTask<string?>>? BearerTokenProvider { get; set; }
}
