namespace Fargo.Application.Shared.Identity;

public sealed record PasswordUpdateDto(
    string NewPassword,
    string CurrentPassword);
