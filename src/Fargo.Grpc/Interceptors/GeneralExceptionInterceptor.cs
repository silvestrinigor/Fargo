using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fargo.Grpc.Interceptors;

public sealed class GeneralExceptionInterceptor(
    ILogger<GeneralExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception while executing gRPC method {Method}",
                context.Method);

            throw new RpcException(
                new Status(
                    StatusCode.Internal,
                    "An unexpected error occurred."));
        }
    }
}
