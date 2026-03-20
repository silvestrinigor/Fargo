using System.Net.Http.Headers;

namespace Fargo.Web.Api;

public sealed class FargoApiAuthorizationHandler(ClientSessionAccessor sessionAccessor) : DelegatingHandler
{
    private readonly ClientSessionAccessor sessionAccessor = sessionAccessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = sessionAccessor.Session?.AccessToken;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
