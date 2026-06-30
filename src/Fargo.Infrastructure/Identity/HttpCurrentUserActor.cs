using Fargo.Application.Identity;
using Fargo.Core.Shared.Actors;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

public sealed class HttpCurrentUserActor(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated == true;

    public ActorId ActorId
    {
        get
        {
            if (!IsAuthenticated)
            {
                return ActorId.Empty;
            }

            var id =
                Principal!.FindFirstValue(ClaimTypes.NameIdentifier) ??
                Principal!.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var actorGuid = Guid.TryParse(id, out var guid) ? guid : Guid.Empty;

            return new ActorId(actorGuid, ActorType.User);
        }
    }
}
