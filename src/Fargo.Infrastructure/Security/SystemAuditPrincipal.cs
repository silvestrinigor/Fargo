using Fargo.Application.Authentication;

namespace Fargo.Infrastructure.Security;

public sealed class SystemAuditPrincipal : IAuditPrincipal
{
    public static Guid SystemGuid { get; } = new("00000000-0000-0000-0000-000000000001");

    public Guid ActorGuid => SystemGuid;
}
