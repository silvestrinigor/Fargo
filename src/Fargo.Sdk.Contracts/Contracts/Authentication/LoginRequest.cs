namespace Fargo.Sdk.Contracts.Authentication;

/// <summary>Represents a login request.</summary>
public sealed record LoginRequest(string Nameid, string Password);
