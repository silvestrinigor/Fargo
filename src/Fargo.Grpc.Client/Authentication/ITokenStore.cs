namespace Fargo.Grpc.Client.Authentication;

public interface ITokenStore
{
    string? AccessToken { get; }

    string? RefreshToken { get; }

    DateTimeOffset? ExpiresAt { get; }

    void SetTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt);

    void Clear();
}
