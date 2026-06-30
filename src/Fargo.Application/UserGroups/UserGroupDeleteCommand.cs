namespace Fargo.Application.UserGroups;

/// <summary>
/// Command used to delete a user group.
/// </summary>
public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;
