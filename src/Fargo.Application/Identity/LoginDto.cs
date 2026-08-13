namespace Fargo.Application.Identity;

public sealed record LoginDto(
    string Nameid,
    string Password
);
