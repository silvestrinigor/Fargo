namespace Fargo.Sdk.Contracts.Authentication;

/// <summary>Represents the password payload inside a password change request.</summary>
public sealed record PasswordUpdateDto(string NewPassword, string? CurrentPassword = null);
