namespace Fargo.Application.Authentication;

/// <summary>
/// Wire shape for changing the current user's password.
/// </summary>
public sealed record PasswordUpdateRequest(string NewPassword, string? CurrentPassword = null);
