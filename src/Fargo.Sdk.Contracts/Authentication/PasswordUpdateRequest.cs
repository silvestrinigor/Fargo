namespace Fargo.Sdk.Contracts.Authentication;

/// <summary>Represents the password payload inside a password change request.</summary>
public sealed record PasswordUpdateRequest(string NewPassword, string? CurrentPassword = null);
