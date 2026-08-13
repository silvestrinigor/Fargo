using Fargo.Application.Common;

namespace Fargo.Application.Users;

public sealed record UserSingleQuery(Guid UserGuid) : IQuery<UserDto?>;
