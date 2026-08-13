using Fargo.Application.Identity;
using Fargo.Core.Shared.Actors;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

public sealed class CurrentUserActorHttp(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated == true;

    public Guid Guid
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

            var actorGuid = Guid.TryParse(id, out var guid) ? guid : Guid.Empty;

            return actorGuid;
        }
    }

    public ActorType ActorType => ActorType.User;
}
