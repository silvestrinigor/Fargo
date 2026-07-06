using Fargo.Application.Shared.UserGroups;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Command used to create a new user group from an API creation payload.
/// </summary>
public sealed record UserGroupCreateCommand(UserGroupCreateDto Create) : ICommand<Guid>;
