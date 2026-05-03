namespace Fargo.Sdk.Contracts.Authentication;

/// <summary>Represents a login request.</summary>
public sealed record LoginDto(string Nameid, string Password);
