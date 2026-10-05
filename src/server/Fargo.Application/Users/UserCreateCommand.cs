using Fargo.Application.Common;

namespace Fargo.Application.Users;

/// <summary>
/// Command used to create a new user.
/// </summary>
public sealed record UserCreateCommand(
    UserCreateDto Create
) : ICommand<Guid>;
