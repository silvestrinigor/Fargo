using Fargo.Application.Authentication;

namespace Fargo.Infrastructure.Security;

public sealed class CurrentAuditPrincipal(ICurrentUser currentUser) : IAuditPrincipal
{
    public Guid ActorGuid => currentUser.UserGuid;
}
