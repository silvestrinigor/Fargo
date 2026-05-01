namespace Fargo.Api.Contracts.Authentication;

/// <summary>Represents a refresh token payload used for token refresh and logout requests.</summary>
public sealed record RefreshDto(string RefreshToken);
