namespace Fargo.Application.Operations.Identity.Dtos;

public sealed record IdentityLoginDto(
    string Nameid,
    string Password
);
