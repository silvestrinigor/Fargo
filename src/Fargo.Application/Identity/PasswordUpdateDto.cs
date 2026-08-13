namespace Fargo.Application.Identity;

public sealed record PasswordUpdateDto(
    string Nameid,
    string NewPassword,
    string CurrentPassword);
