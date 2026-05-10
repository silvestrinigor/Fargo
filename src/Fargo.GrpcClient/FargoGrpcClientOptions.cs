namespace Fargo.GrpcClient;

public sealed class FargoGrpcClientOptions
{
    public Uri? Server { get; set; }

    public string? AccessToken { get; set; }

    public string? ApiKey { get; set; }

    public TimeSpan? DefaultDeadline { get; set; } = TimeSpan.FromSeconds(30);

    public int MaxRetries { get; set; } = 1;
}
