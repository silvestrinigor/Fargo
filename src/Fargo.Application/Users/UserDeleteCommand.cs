namespace Fargo.Application.Users;

/// <summary>
/// Command used to delete a user.
/// </summary>
public sealed record UserDeleteCommand(
    Guid UserGuid
) : ICommand;