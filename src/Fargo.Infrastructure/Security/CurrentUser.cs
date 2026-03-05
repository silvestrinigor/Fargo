using Fargo.Application.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security
{
    public sealed class CurrentUser(
            IHttpContextAccessor httpContextAccessor
            )
        : ICurrentUser
    {
        private readonly IHttpContextAccessor _http = httpContextAccessor;

        private ClaimsPrincipal? Principal => _http.HttpContext?.User;

        public bool IsAuthenticated
            => Principal?.Identity?.IsAuthenticated == true;

        public Guid UserGuid
        {
            get
            {
                if (!IsAuthenticated)
                    return Guid.Empty;

                var id =
                    Principal!.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    Principal!.FindFirstValue(JwtRegisteredClaimNames.Sub);

                return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
            }
        }
    }
}