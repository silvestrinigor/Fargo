using Fargo.Application.Shared.Users;

namespace Fargo.Application.Users;

public sealed record UserSingleQuery(
    Guid UserGuid
) : IQuery<UserDto?>;
