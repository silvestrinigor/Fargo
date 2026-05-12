using Fargo.Application.Identity;

namespace Fargo.Infrastructure.Security;

public sealed class CurrentAuditPrincipal(ICurrentUser currentUser) : IAuditPrincipal
{
    public Guid ActorGuid => currentUser.UserGuid;
}
