// AuthenticatedSession.cs
namespace Fargo.Web.Security;

public sealed class AuthenticatedSession
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}