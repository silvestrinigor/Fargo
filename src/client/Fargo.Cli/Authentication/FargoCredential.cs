using ktsu.CredentialCache;

public sealed class FargoCredential : Credential
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset AccessTokenExpiresAt { get; init; }
}
