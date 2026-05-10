using Grpc.Core;

namespace Fargo.GrpcClient;

public sealed class FargoGrpcCallExecutor(FargoGrpcClientOptions options)
{
    public async Task<TResponse> UnaryAsync<TResponse>(
        Func<Metadata, DateTime?, CancellationToken, AsyncUnaryCall<TResponse>> call,
        CancellationToken cancellationToken = default)
    {
        var attempts = Math.Max(0, options.MaxRetries) + 1;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var grpcCall = call(CreateMetadata(), CreateDeadline(), cancellationToken);
                return await grpcCall.ResponseAsync;
            }
            catch (RpcException exception) when (attempt < attempts && IsTransient(exception.StatusCode))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken);
            }
        }
    }

    private Metadata CreateMetadata()
    {
        var metadata = new Metadata();

        if (!string.IsNullOrWhiteSpace(options.AccessToken))
        {
            metadata.Add("authorization", $"Bearer {options.AccessToken}");
        }

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            metadata.Add("x-api-key", options.ApiKey);
        }

        return metadata;
    }

    private DateTime? CreateDeadline()
        => options.DefaultDeadline is { } deadline
            ? DateTime.UtcNow.Add(deadline)
            : null;

    private static bool IsTransient(StatusCode statusCode)
        => statusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded;
}
