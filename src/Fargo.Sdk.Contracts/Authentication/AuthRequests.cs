namespace Fargo.Sdk.Contracts.Authentication;

/// <summary>Represents a login request.</summary>
public sealed record LoginRequest(string Nameid, string Password);

/// <summary>Represents a logout request.</summary>
public sealed record LogoutRequest(string RefreshToken);

/// <summary>Represents a refresh-token request.</summary>
public sealed record RefreshRequest(string RefreshToken);

/// <summary>Represents the password payload inside a password change request.</summary>
public sealed record PasswordUpdateRequest(string NewPassword, string? CurrentPassword = null);

/// <summary>Represents a password change request.</summary>
public sealed record PasswordChangeRequest(PasswordUpdateRequest Passwords);

