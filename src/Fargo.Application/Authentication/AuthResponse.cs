using Fargo.Domain;

namespace Fargo.Application.Authentication;

/// <summary>
/// Wire-friendly authentication response (strings instead of Token value objects).
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses);

/// <summary>Conversions for <see cref="AuthResult"/> to the wire response.</summary>
public static class AuthResponseMappings
{
    public static AuthResponse ToResponse(this AuthResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new AuthResponse(
            result.AccessToken.Value,
            result.RefreshToken.Value,
            result.ExpiresAt,
            result.IsAdmin,
            result.PermissionActions,
            result.PartitionAccesses);
    }
}
