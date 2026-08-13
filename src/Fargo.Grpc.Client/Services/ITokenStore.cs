namespace Fargo.Grpc.Client.Services;

public interface ITokenStore
{
    string? AccessToken { get; }

    string? RefreshToken { get; }

    void SetTokens(string accessToken, string refreshToken);

    void Clear();
}
