using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fargo.Grpc.Client.Interceptors;

public sealed class AuthenticationInterceptor(
    IAccessTokenProvider accessTokenProvider)
    : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        return CreateCall(
            request,
            context,
            continuation);
    }

    private AsyncUnaryCall<TResponse> CreateCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        var responseAsync = InvokeAsync(
            request,
            context,
            continuation);

        return new AsyncUnaryCall<TResponse>(
            responseAsync,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }

    private async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        var token = await accessTokenProvider.GetAccessTokenAsync(
            context.Options.CancellationToken);

        if (string.IsNullOrWhiteSpace(token))
        {
            var call = continuation(request, context);

            return await call.ResponseAsync;
        }

        var headers = CloneMetadata(context.Options.Headers);

        headers.Add(
            "Authorization",
            $"Bearer {token}");

        var options = context.Options.WithHeaders(headers);

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);

        var authenticatedCall = continuation(
            request,
            newContext);

        return await authenticatedCall.ResponseAsync;
    }

    private static Metadata CloneMetadata(Metadata? source)
    {
        var metadata = new Metadata();

        if (source is null)
        {
            return metadata;
        }

        foreach (var entry in source)
        {
            metadata.Add(entry);
        }

        return metadata;
    }
}
