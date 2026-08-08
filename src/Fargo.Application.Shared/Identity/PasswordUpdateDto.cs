namespace Fargo.Application.Shared.Identity;

public sealed record PasswordUpdateDto(
    string Nameid,
    string NewPassword,
    string CurrentPassword);
