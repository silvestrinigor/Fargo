using Fargo.Application.Shared.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Command used to update an existing user from an API update payload.
/// </summary>
public sealed record UserUpdateCommand(
    Guid UserGuid,
    UserUpdateDto Update
) : ICommand;