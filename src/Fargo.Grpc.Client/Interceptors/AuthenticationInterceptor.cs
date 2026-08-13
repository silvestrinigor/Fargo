using Fargo.Grpc.Client.Services;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fargo.Grpc.Client.Interceptors;

public sealed class AuthenticationInterceptor : Interceptor
{
    private readonly ITokenStore tokenStore;

    public AuthenticationInterceptor(ITokenStore tokenStore)
    {
        this.tokenStore = tokenStore;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var token = tokenStore.AccessToken;

        if (string.IsNullOrWhiteSpace(token))
        {
            return continuation(request, context);
        }

        var headers = context.Options.Headers ?? [];

        headers.Add("Authorization", $"Bearer {token}");

        var options = context.Options.WithHeaders(headers);

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);

        return continuation(request, newContext);
    }
}
