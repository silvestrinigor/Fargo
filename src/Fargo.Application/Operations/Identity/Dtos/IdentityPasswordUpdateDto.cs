using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Operations.Identity.Dtos;

public sealed record IdentityPasswordUpdateDto(
    string Nameid,
    Password NewPassword,
    string CurrentPassword
);
