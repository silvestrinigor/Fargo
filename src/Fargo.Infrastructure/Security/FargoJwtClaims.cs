namespace Fargo.Infrastructure.Security;

public static class FargoJwtClaims
{
    public const string AuthVersion = "fargo_auth_version";

    /// <summary>
    /// Absent or "user" means a human user; "service" means a service account (client-credentials flow).
    /// </summary>
    public const string ActorType = "fargo_actor_type";

    public const string ActorTypeService = "service";
}
