using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Dtos.UserGroups;

public sealed record UserGroupUpdateDto(
    Nameid? Nameid = null,
    Description? Description = null
);