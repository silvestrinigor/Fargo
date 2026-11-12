namespace Fargo.Infrastructure.Security;

public static class FargoJwtClaims
{
    public const string AuthVersion = "fargo_auth_version";

    /// <summary>
    /// Claim carrying the actor type of the token subject. Absent or "user" means
    /// a human user; "service" means a service account (client-credentials flow).
    /// </summary>
    public const string ActorType = "fargo_actor_type";

    /// <summary>Value of <see cref="ActorType"/> for a service-account token.</summary>
    public const string ActorTypeService = "service";
}
