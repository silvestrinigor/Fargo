using Fargo.Application.Authentication;
using Microsoft.AspNetCore.Http;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Provides the implementation of <see cref="ICurrentApiClient"/> based on
/// the <c>ApiClientGuid</c> item set by <c>ApiClientMiddleware</c>.
/// Returns <see cref="Guid.Empty"/> when no API client key was provided.
/// </summary>
public sealed class CurrentApiClient(IHttpContextAccessor httpContextAccessor) : ICurrentApiClient
{
    public Guid ApiClientGuid
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return Guid.Empty;
            }

            return context.Items.TryGetValue("ApiClientGuid", out var value) && value is Guid guid
                ? guid
                : Guid.Empty;
        }
    }
}
