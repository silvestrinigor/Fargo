using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Fargo.GrpcApi;

public sealed class FargoGrpcExceptionInterceptor(IWebHostEnvironment environment) : Interceptor
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
        catch (Exception exception)
        {
            throw ToRpcException(exception, context);
        }
    }

    private RpcException ToRpcException(Exception exception, ServerCallContext context)
    {
        if (GrpcProblemDetailsRegistry.TryGetDefinition(exception.GetType(), out var definition))
        {
            return new RpcException(
                new Status(ToGrpcStatusCode(definition.StatusCode), exception.Message),
                CreateTrailers(definition.StatusCode, definition.Title, exception.Message, definition.Type, context));
        }

        const int statusCode = 500;
        var detail = environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.";

        return new RpcException(
            new Status(StatusCode.Internal, detail),
            CreateTrailers(statusCode, "Internal server error", detail, "server/internal-error", context));
    }

    private static Metadata CreateTrailers(
        int status,
        string title,
        string detail,
        string type,
        ServerCallContext context)
        =>
        [
            new("problem-status", status.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("problem-title", title),
            new("problem-detail", detail),
            new("problem-type", type),
            new("trace-id", context.GetHttpContext().TraceIdentifier)
        ];

    private static StatusCode ToGrpcStatusCode(int statusCode)
        => statusCode switch
        {
            400 => StatusCode.InvalidArgument,
            401 => StatusCode.Unauthenticated,
            403 => StatusCode.PermissionDenied,
            404 => StatusCode.NotFound,
            409 => StatusCode.AlreadyExists,
            _ => StatusCode.Internal
        };
}
