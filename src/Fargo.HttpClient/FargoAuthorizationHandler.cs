using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Fargo.HttpClient;

internal sealed class FargoAuthorizationHandler(
    IOptions<FargoHttpClientOptions> options)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var token = options.Value.BearerTokenProvider is null
                ? options.Value.BearerToken
                : await options.Value.BearerTokenProvider(cancellationToken);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
