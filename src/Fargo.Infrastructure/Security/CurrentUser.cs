using Fargo.Application.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Provides the implementation of <see cref="ICurrentUser"/> based on the
/// current HTTP request context.
/// </summary>
/// <remarks>
/// This implementation extracts authentication and identity information
/// from the <see cref="HttpContext"/> associated with the current request.
///
/// The user identifier is resolved from standard claims present in the
/// authenticated <see cref="ClaimsPrincipal"/>, typically provided by
/// authentication middleware such as JWT Bearer authentication.
///
/// The following claim types are checked to determine the user identifier:
/// <list type="bullet">
/// <item>
/// <description><see cref="ClaimTypes.NameIdentifier"/></description>
/// </item>
/// <item>
/// <description><see cref="JwtRegisteredClaimNames.Sub"/></description>
/// </item>
/// </list>
///
/// If the request is not authenticated or the identifier claim cannot
/// be parsed as a <see cref="Guid"/>, <see cref="Guid.Empty"/> is returned.
/// </remarks>
public sealed class CurrentUser(
        IHttpContextAccessor httpContextAccessor
        )
    : ICurrentUser
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    /// <summary>
    /// Gets the <see cref="ClaimsPrincipal"/> associated with the current HTTP request.
    /// </summary>
    /// <remarks>
    /// This value may be <see langword="null"/> when there is no active HTTP context,
    /// such as during background processing or non-request execution paths.
    /// </remarks>
    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    /// <summary>
    /// Gets a value indicating whether the current request is authenticated.
    /// </summary>
    /// <remarks>
    /// Authentication is determined based on the <see cref="ClaimsIdentity.IsAuthenticated"/>
    /// property of the current <see cref="ClaimsPrincipal"/>.
    /// </remarks>
    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Gets the unique identifier of the authenticated user.
    /// </summary>
    /// <remarks>
    /// The identifier is resolved from the following claims, in order:
    /// <list type="number">
    /// <item>
    /// <description><see cref="ClaimTypes.NameIdentifier"/></description>
    /// </item>
    /// <item>
    /// <description><see cref="JwtRegisteredClaimNames.Sub"/></description>
    /// </item>
    /// </list>
    ///
    /// If the request is not authenticated, or if the claim value cannot
    /// be parsed into a valid <see cref="Guid"/>, this property returns
    /// <see cref="Guid.Empty"/>.
    /// </remarks>
    public Guid UserGuid
    {
        get
        {
            if (!IsAuthenticated)
            {
                return Guid.Empty;
            }

            var id =
                Principal!.FindFirstValue(ClaimTypes.NameIdentifier) ??
                Principal!.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
        }
    }
}
