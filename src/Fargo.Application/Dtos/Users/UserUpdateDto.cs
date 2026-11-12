using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Dtos.Users;

public sealed record UserUpdateDto(
    Nameid? Nameid = null,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null
);