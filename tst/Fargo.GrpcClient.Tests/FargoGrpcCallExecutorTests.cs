using Fargo.GrpcContracts;
using Grpc.Core;

namespace Fargo.GrpcClient.Tests;

public sealed class FargoGrpcCallExecutorTests
{
    [Fact]
    public async Task UnaryAsync_Should_Add_Metadata_And_Deadline()
    {
        var options = new FargoGrpcClientOptions
        {
            AccessToken = "access-token",
            ApiKey = "api-key",
            DefaultDeadline = TimeSpan.FromMinutes(1),
            MaxRetries = 0
        };
        var sut = new FargoGrpcCallExecutor(options);
        Metadata? observedHeaders = null;
        DateTime? observedDeadline = null;

        var result = await sut.UnaryAsync(
            (headers, deadline, _) =>
            {
                observedHeaders = headers;
                observedDeadline = deadline;
                return Completed(new GuidResult { Guid = Guid.NewGuid().ToString() });
            });

        Assert.False(string.IsNullOrWhiteSpace(result.Guid));
        Assert.NotNull(observedHeaders);
        Assert.Equal("Bearer access-token", observedHeaders!.First(header => header.Key == "authorization").Value);
        Assert.Equal("api-key", observedHeaders.First(header => header.Key == "x-api-key").Value);
        Assert.NotNull(observedDeadline);
        Assert.True(observedDeadline > DateTime.UtcNow);
    }

    [Fact]
    public async Task UnaryAsync_Should_Retry_Transient_RpcException()
    {
        var options = new FargoGrpcClientOptions
        {
            DefaultDeadline = null,
            MaxRetries = 1
        };
        var sut = new FargoGrpcCallExecutor(options);
        var calls = 0;

        var result = await sut.UnaryAsync<GuidResult>(
            (_, _, _) =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new RpcException(new Status(StatusCode.Unavailable, "Unavailable"));
                }

                return Completed(new GuidResult { Guid = "ok" });
            });

        Assert.Equal("ok", result.Guid);
        Assert.Equal(2, calls);
    }

    private static AsyncUnaryCall<T> Completed<T>(T response)
        => new(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            static () => new Status(StatusCode.OK, string.Empty),
            static () => new Metadata(),
            static () => { });
}
