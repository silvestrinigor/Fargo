namespace Fargo.Application.Authentication;

/// <summary>
/// Wire shape for a login request.
/// </summary>
public sealed record LoginRequest(string Nameid, string Password);
