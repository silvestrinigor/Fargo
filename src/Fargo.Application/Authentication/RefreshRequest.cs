namespace Fargo.Application.Authentication;

/// <summary>
/// Wire shape for refreshing an access token.
/// </summary>
public sealed record RefreshRequest(string RefreshToken);
