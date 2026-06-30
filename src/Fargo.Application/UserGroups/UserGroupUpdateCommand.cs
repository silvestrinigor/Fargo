using Fargo.Application.Shared.UserGroups;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Command used to update an existing user group from an API update payload.
/// </summary>
public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid,
    UserGroupUpdateDto Update
) : ICommand;