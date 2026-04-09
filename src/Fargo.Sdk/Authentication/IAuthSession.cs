namespace Fargo.Sdk.Authentication;

public interface IAuthSession
{
    public string? Nameid { get; }

    public string? AccessToken { get; }

    public string? RefreshToken { get; }

    public DateTimeOffset? ExpiresAt { get;  }

    public bool IsAuthenticated { get; }
}
