using Fargo.Application.Shared.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Command used to create a new user.
/// </summary>
public sealed record UserCreateCommand(
    UserCreateDto Create
) : ICommand<Guid>;
