using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Dtos.Identity;

public sealed record IdentityPasswordUpdateDto(
    string Nameid,
    Password NewPassword,
    string CurrentPassword
);
