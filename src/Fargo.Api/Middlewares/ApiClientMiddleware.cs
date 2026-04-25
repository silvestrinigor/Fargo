using Fargo.Application.ApiClients;
using Fargo.Domain.ApiClients;
using Microsoft.Extensions.Options;

namespace Fargo.Api.Middlewares;

public sealed class ApiClientMiddleware(RequestDelegate next, IOptions<ApiClientOptions> options)
{
    private static readonly PathString AuthenticationPath = new("/authentication");

    public async Task InvokeAsync(HttpContext context, IApiClientQueryRepository repo)
    {
        if (context.Request.Headers.TryGetValue("X-Api-Key", out var rawKey)
            && !string.IsNullOrEmpty(rawKey))
        {
            var hash = ApiKeyGenerator.Hash(rawKey!);
            var guid = await repo.FindActiveGuidByKeyHash(hash, context.RequestAborted);

            if (guid.HasValue)
                context.Items["ApiClientGuid"] = guid.Value;
        }

        if (options.Value.EnforceApiClient
            && !context.Items.ContainsKey("ApiClientGuid")
            && !context.Request.Path.StartsWithSegments(AuthenticationPath))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
