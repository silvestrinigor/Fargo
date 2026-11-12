using Fargo.Application.Common.Actors;
using Fargo.Core.Entities.Actors;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

public sealed class CurrentUserActor(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated == true;

    public Guid Guid => Principal.GetFargoActorGuid();

    public ActorType ActorType => Principal.GetFargoActorType();
}
