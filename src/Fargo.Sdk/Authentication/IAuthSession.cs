namespace Fargo.Sdk.Authentication;

public interface IAuthSession
{
    string? Nameid { get; }

    string? AccessToken { get; }

    string? RefreshToken { get; }

    DateTimeOffset? ExpiresAt { get; }

    bool IsAuthenticated { get; }
}
